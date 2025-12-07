# Integration API - Authentication & Key Management Specification

## Document Purpose

Complete implementation specification for API Key-based authentication infrastructure for the B2B E-Commerce Platform Integration API. This document covers database design, data access layer, services, and admin APIs for managing integration clients and API keys.

**Target**: .NET 8, EF Core 8, Clean Architecture  
**Version**: 1.0  
**Date**: December 2025  
**Related**: B2B_Technical_Architecture_Overview.md, Infrastructure_Layer_Specification.md

---

## Table of Contents

1. [Overview](#overview)
2. [Database Design](#database-design)
3. [Domain Layer](#domain-layer)
4. [Application Layer](#application-layer)
5. [Infrastructure Layer](#infrastructure-layer)
6. [API Layer](#api-layer)
7. [Authentication Handler](#authentication-handler)
8. [Middleware Components](#middleware-components)
9. [Configuration](#configuration)
10. [Security Considerations](#security-considerations)

---

## Overview

### Authentication Strategy

The Integration API uses **API Key authentication** for system-to-system communication, distinct from the main B2B API's JWT-based user authentication.

```
┌─────────────────┐     X-API-Key Header      ┌─────────────────────┐
│   LOGO ERP      │ ─────────────────────────▶│   Integration API   │
│   (Client)      │                           │                     │
└─────────────────┘                           └─────────────────────┘
                                                        │
                                                        ▼
                                              ┌─────────────────────┐
                                              │  Validate API Key   │
                                              │  Check Permissions  │
                                              │  Rate Limit Check   │
                                              │  IP Whitelist Check │
                                              │  Audit Log          │
                                              └─────────────────────┘
```

### Key Features

- Unique API key per integration client
- Hashed key storage (like passwords)
- Granular permission scopes
- Rate limiting per key
- Optional IP whitelisting
- Comprehensive audit logging
- Key rotation support
- Expiration management

---

## Database Design

### Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              ApiClient                                  │
│─────────────────────────────────────────────────────────────────────────│
│ Id (PK)                                                                 │
│ Name                                                                    │
│ Description                                                             │
│ ContactEmail                                                            │
│ ContactPhone                                                            │
│ IsActive                                                                │
│ CreatedAt, CreatedBy, UpdatedAt, UpdatedBy                             │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │ 1:N
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                              ApiKey                                     │
│─────────────────────────────────────────────────────────────────────────│
│ Id (PK)                                                                 │
│ ApiClientId (FK)                                                        │
│ KeyHash (hashed value)                                                  │
│ KeyPrefix (first 8 chars for identification)                            │
│ Name                                                                    │
│ ExpiresAt                                                               │
│ LastUsedAt                                                              │
│ LastUsedIp                                                              │
│ RateLimitPerMinute                                                      │
│ IsActive                                                                │
│ RevokedAt, RevokedBy, RevocationReason                                  │
│ CreatedAt, CreatedBy                                                    │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │ 1:N                    │ 1:N
                                 ▼                        ▼
┌─────────────────────────────────────────┐  ┌────────────────────────────┐
│         ApiKeyPermission                │  │    ApiKeyIpWhitelist       │
│─────────────────────────────────────────│  │────────────────────────────│
│ Id (PK)                                 │  │ Id (PK)                    │
│ ApiKeyId (FK)                           │  │ ApiKeyId (FK)              │
│ Scope (e.g., "products:write")          │  │ IpAddress                  │
│ CreatedAt                               │  │ Description                │
└─────────────────────────────────────────┘  │ CreatedAt                  │
                                             └────────────────────────────┘
                                 │
                                 ▼ 1:N
┌─────────────────────────────────────────────────────────────────────────┐
│                          ApiKeyUsageLog                                 │
│─────────────────────────────────────────────────────────────────────────│
│ Id (PK)                                                                 │
│ ApiKeyId (FK)                                                           │
│ Endpoint                                                                │
│ HttpMethod                                                              │
│ IpAddress                                                               │
│ UserAgent                                                               │
│ RequestTimestamp                                                        │
│ ResponseStatusCode                                                      │
│ ResponseTimeMs                                                          │
│ ErrorMessage                                                            │
└─────────────────────────────────────────────────────────────────────────┘
```

### Table Specifications

#### ApiClients Table

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Identity | Primary key |
| Name | nvarchar(100) | Required, Unique | Client name (e.g., "LOGO ERP Production") |
| Description | nvarchar(500) | Nullable | Description of integration purpose |
| ContactEmail | nvarchar(255) | Required | Contact email for notifications |
| ContactPhone | nvarchar(20) | Nullable | Contact phone number |
| IsActive | bit | Required, Default: true | Client status |
| CreatedAt | datetime2 | Required | Creation timestamp (UTC) |
| CreatedBy | nvarchar(100) | Nullable | Creator identifier |
| UpdatedAt | datetime2 | Nullable | Last update timestamp (UTC) |
| UpdatedBy | nvarchar(100) | Nullable | Updater identifier |
| IsDeleted | bit | Required, Default: false | Soft delete flag |
| DeletedAt | datetime2 | Nullable | Deletion timestamp |
| DeletedBy | nvarchar(100) | Nullable | Deleter identifier |

#### ApiKeys Table

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Identity | Primary key |
| ApiClientId | int | FK, Required | Reference to ApiClient |
| KeyHash | nvarchar(256) | Required | SHA-256 hash of the API key |
| KeyPrefix | nvarchar(8) | Required | First 8 chars for identification (e.g., "ek_live_") |
| Name | nvarchar(100) | Required | Key name (e.g., "Production Key 2024") |
| ExpiresAt | datetime2 | Nullable | Key expiration (null = never expires) |
| LastUsedAt | datetime2 | Nullable | Last usage timestamp |
| LastUsedIp | nvarchar(45) | Nullable | Last usage IP address |
| RateLimitPerMinute | int | Required, Default: 500 | Rate limit for this key |
| IsActive | bit | Required, Default: true | Key status |
| RevokedAt | datetime2 | Nullable | Revocation timestamp |
| RevokedBy | nvarchar(100) | Nullable | Revoker identifier |
| RevocationReason | nvarchar(500) | Nullable | Reason for revocation |
| CreatedAt | datetime2 | Required | Creation timestamp (UTC) |
| CreatedBy | nvarchar(100) | Nullable | Creator identifier |

#### ApiKeyPermissions Table

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Identity | Primary key |
| ApiKeyId | int | FK, Required | Reference to ApiKey |
| Scope | nvarchar(100) | Required | Permission scope |
| CreatedAt | datetime2 | Required | Creation timestamp (UTC) |

**Permission Scopes:**

```
products:read          - Read product data
products:write         - Create/update products
stock:read             - Read stock levels
stock:write            - Update stock levels
prices:read            - Read pricing data
prices:write           - Update prices
customers:read         - Read customer data
customers:write        - Create/update customers
orders:read            - Read order data
orders:write           - Update order status
invoices:read          - Read invoice data
invoices:write         - Create invoices
webhooks:manage        - Manage webhook subscriptions
```

#### ApiKeyIpWhitelist Table

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, Identity | Primary key |
| ApiKeyId | int | FK, Required | Reference to ApiKey |
| IpAddress | nvarchar(45) | Required | IPv4 or IPv6 address (supports CIDR) |
| Description | nvarchar(200) | Nullable | Description (e.g., "LOGO ERP Server") |
| CreatedAt | datetime2 | Required | Creation timestamp (UTC) |

#### ApiKeyUsageLogs Table

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | bigint | PK, Identity | Primary key (bigint for high volume) |
| ApiKeyId | int | FK, Required, Indexed | Reference to ApiKey |
| Endpoint | nvarchar(500) | Required | Request endpoint path |
| HttpMethod | nvarchar(10) | Required | HTTP method (GET, POST, etc.) |
| IpAddress | nvarchar(45) | Required | Client IP address |
| UserAgent | nvarchar(500) | Nullable | User agent string |
| RequestTimestamp | datetime2 | Required, Indexed | Request timestamp (UTC) |
| ResponseStatusCode | int | Required | HTTP response status code |
| ResponseTimeMs | int | Required | Response time in milliseconds |
| ErrorMessage | nvarchar(1000) | Nullable | Error message if failed |

**Indexes:**

```sql
-- ApiClients
CREATE UNIQUE INDEX IX_ApiClients_Name ON ApiClients(Name) WHERE IsDeleted = 0;
CREATE INDEX IX_ApiClients_IsActive ON ApiClients(IsActive) WHERE IsDeleted = 0;

-- ApiKeys
CREATE INDEX IX_ApiKeys_KeyHash ON ApiKeys(KeyHash);
CREATE INDEX IX_ApiKeys_KeyPrefix ON ApiKeys(KeyPrefix);
CREATE INDEX IX_ApiKeys_ApiClientId ON ApiKeys(ApiClientId);
CREATE INDEX IX_ApiKeys_IsActive_ExpiresAt ON ApiKeys(IsActive, ExpiresAt);

-- ApiKeyPermissions
CREATE INDEX IX_ApiKeyPermissions_ApiKeyId ON ApiKeyPermissions(ApiKeyId);
CREATE UNIQUE INDEX IX_ApiKeyPermissions_ApiKeyId_Scope ON ApiKeyPermissions(ApiKeyId, Scope);

-- ApiKeyIpWhitelist
CREATE INDEX IX_ApiKeyIpWhitelist_ApiKeyId ON ApiKeyIpWhitelist(ApiKeyId);

-- ApiKeyUsageLogs (partitioned by month recommended for production)
CREATE INDEX IX_ApiKeyUsageLogs_ApiKeyId ON ApiKeyUsageLogs(ApiKeyId);
CREATE INDEX IX_ApiKeyUsageLogs_RequestTimestamp ON ApiKeyUsageLogs(RequestTimestamp);
CREATE INDEX IX_ApiKeyUsageLogs_ApiKeyId_RequestTimestamp ON ApiKeyUsageLogs(ApiKeyId, RequestTimestamp);
```

---

## Domain Layer

### Entities

#### ApiClient.cs

```csharp
// File: Domain/Entities/Integration/ApiClient.cs
namespace EnormTech.B2B.Domain.Entities.Integration
{
    public class ApiClient : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public string ContactEmail { get; private set; }
        public string? ContactPhone { get; private set; }
        public bool IsActive { get; private set; }
        
        // Navigation
        private readonly List<ApiKey> _apiKeys = new();
        public IReadOnlyCollection<ApiKey> ApiKeys => _apiKeys.AsReadOnly();
        
        // Factory Method
        public static ApiClient Create(
            string name,
            string contactEmail,
            string? description = null,
            string? contactPhone = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Client name is required");
            
            if (string.IsNullOrWhiteSpace(contactEmail))
                throw new DomainException("Contact email is required");
            
            return new ApiClient
            {
                Name = name.Trim(),
                ContactEmail = contactEmail.Trim().ToLowerInvariant(),
                Description = description?.Trim(),
                ContactPhone = contactPhone?.Trim(),
                IsActive = true
            };
        }
        
        public void Update(string name, string contactEmail, string? description, string? contactPhone)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Client name is required");
            
            if (string.IsNullOrWhiteSpace(contactEmail))
                throw new DomainException("Contact email is required");
            
            Name = name.Trim();
            ContactEmail = contactEmail.Trim().ToLowerInvariant();
            Description = description?.Trim();
            ContactPhone = contactPhone?.Trim();
        }
        
        public void Activate() => IsActive = true;
        
        public void Deactivate()
        {
            IsActive = false;
            // Deactivate all associated keys
            foreach (var key in _apiKeys.Where(k => k.IsActive))
            {
                key.Revoke("Parent client deactivated", "System");
            }
        }
        
        public ApiKey CreateApiKey(
            string keyHash,
            string keyPrefix,
            string name,
            int rateLimitPerMinute = 500,
            DateTime? expiresAt = null,
            string? createdBy = null)
        {
            if (!IsActive)
                throw new DomainException("Cannot create API key for inactive client");
            
            var apiKey = ApiKey.Create(
                this.Id,
                keyHash,
                keyPrefix,
                name,
                rateLimitPerMinute,
                expiresAt,
                createdBy);
            
            _apiKeys.Add(apiKey);
            return apiKey;
        }
    }
}
```

#### ApiKey.cs

```csharp
// File: Domain/Entities/Integration/ApiKey.cs
namespace EnormTech.B2B.Domain.Entities.Integration
{
    public class ApiKey : BaseEntity
    {
        public int ApiClientId { get; private set; }
        public string KeyHash { get; private set; }
        public string KeyPrefix { get; private set; }
        public string Name { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public DateTime? LastUsedAt { get; private set; }
        public string? LastUsedIp { get; private set; }
        public int RateLimitPerMinute { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedBy { get; private set; }
        public string? RevocationReason { get; private set; }
        
        // Navigation
        public ApiClient ApiClient { get; private set; }
        
        private readonly List<ApiKeyPermission> _permissions = new();
        public IReadOnlyCollection<ApiKeyPermission> Permissions => _permissions.AsReadOnly();
        
        private readonly List<ApiKeyIpWhitelist> _ipWhitelist = new();
        public IReadOnlyCollection<ApiKeyIpWhitelist> IpWhitelist => _ipWhitelist.AsReadOnly();
        
        // Factory Method
        public static ApiKey Create(
            int apiClientId,
            string keyHash,
            string keyPrefix,
            string name,
            int rateLimitPerMinute = 500,
            DateTime? expiresAt = null,
            string? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(keyHash))
                throw new DomainException("Key hash is required");
            
            if (string.IsNullOrWhiteSpace(keyPrefix))
                throw new DomainException("Key prefix is required");
            
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Key name is required");
            
            if (rateLimitPerMinute <= 0)
                throw new DomainException("Rate limit must be positive");
            
            if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
                throw new DomainException("Expiration date must be in the future");
            
            return new ApiKey
            {
                ApiClientId = apiClientId,
                KeyHash = keyHash,
                KeyPrefix = keyPrefix,
                Name = name.Trim(),
                RateLimitPerMinute = rateLimitPerMinute,
                ExpiresAt = expiresAt,
                IsActive = true,
                CreatedBy = createdBy
            };
        }
        
        public bool IsValid()
        {
            if (!IsActive) return false;
            if (RevokedAt.HasValue) return false;
            if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow) return false;
            return true;
        }
        
        public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
        
        public bool IsRevoked() => RevokedAt.HasValue;
        
        public void RecordUsage(string ipAddress)
        {
            LastUsedAt = DateTime.UtcNow;
            LastUsedIp = ipAddress;
        }
        
        public void UpdateRateLimit(int rateLimitPerMinute)
        {
            if (rateLimitPerMinute <= 0)
                throw new DomainException("Rate limit must be positive");
            
            RateLimitPerMinute = rateLimitPerMinute;
        }
        
        public void UpdateExpiration(DateTime? expiresAt)
        {
            if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
                throw new DomainException("Expiration date must be in the future");
            
            ExpiresAt = expiresAt;
        }
        
        public void Revoke(string reason, string revokedBy)
        {
            if (RevokedAt.HasValue)
                throw new DomainException("Key is already revoked");
            
            IsActive = false;
            RevokedAt = DateTime.UtcNow;
            RevokedBy = revokedBy;
            RevocationReason = reason;
        }
        
        public void AddPermission(string scope)
        {
            if (_permissions.Any(p => p.Scope == scope))
                throw new DomainException($"Permission '{scope}' already exists");
            
            _permissions.Add(ApiKeyPermission.Create(this.Id, scope));
        }
        
        public void RemovePermission(string scope)
        {
            var permission = _permissions.FirstOrDefault(p => p.Scope == scope);
            if (permission != null)
            {
                _permissions.Remove(permission);
            }
        }
        
        public bool HasPermission(string scope)
        {
            // Check for exact match or wildcard
            return _permissions.Any(p => 
                p.Scope == scope || 
                p.Scope == "*" ||
                (scope.Contains(':') && p.Scope == scope.Split(':')[0] + ":*"));
        }
        
        public void AddIpToWhitelist(string ipAddress, string? description = null)
        {
            if (_ipWhitelist.Any(ip => ip.IpAddress == ipAddress))
                throw new DomainException($"IP address '{ipAddress}' already whitelisted");
            
            _ipWhitelist.Add(ApiKeyIpWhitelist.Create(this.Id, ipAddress, description));
        }
        
        public void RemoveIpFromWhitelist(string ipAddress)
        {
            var ip = _ipWhitelist.FirstOrDefault(x => x.IpAddress == ipAddress);
            if (ip != null)
            {
                _ipWhitelist.Remove(ip);
            }
        }
        
        public bool IsIpAllowed(string ipAddress)
        {
            // If no whitelist configured, allow all
            if (!_ipWhitelist.Any()) return true;
            
            // Check exact match or CIDR range
            return _ipWhitelist.Any(w => 
                w.IpAddress == ipAddress || 
                IpAddressHelper.IsInRange(ipAddress, w.IpAddress));
        }
    }
}
```

#### ApiKeyPermission.cs

```csharp
// File: Domain/Entities/Integration/ApiKeyPermission.cs
namespace EnormTech.B2B.Domain.Entities.Integration
{
    public class ApiKeyPermission : BaseEntity
    {
        public int ApiKeyId { get; private set; }
        public string Scope { get; private set; }
        
        // Navigation
        public ApiKey ApiKey { get; private set; }
        
        public static ApiKeyPermission Create(int apiKeyId, string scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
                throw new DomainException("Scope is required");
            
            if (!IntegrationPermissionScopes.IsValidScope(scope))
                throw new DomainException($"Invalid permission scope: {scope}");
            
            return new ApiKeyPermission
            {
                ApiKeyId = apiKeyId,
                Scope = scope.Trim().ToLowerInvariant()
            };
        }
    }
}
```

#### ApiKeyIpWhitelist.cs

```csharp
// File: Domain/Entities/Integration/ApiKeyIpWhitelist.cs
namespace EnormTech.B2B.Domain.Entities.Integration
{
    public class ApiKeyIpWhitelist : BaseEntity
    {
        public int ApiKeyId { get; private set; }
        public string IpAddress { get; private set; }
        public string? Description { get; private set; }
        
        // Navigation
        public ApiKey ApiKey { get; private set; }
        
        public static ApiKeyIpWhitelist Create(int apiKeyId, string ipAddress, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new DomainException("IP address is required");
            
            if (!IpAddressHelper.IsValidIpOrCidr(ipAddress))
                throw new DomainException($"Invalid IP address or CIDR: {ipAddress}");
            
            return new ApiKeyIpWhitelist
            {
                ApiKeyId = apiKeyId,
                IpAddress = ipAddress.Trim(),
                Description = description?.Trim()
            };
        }
    }
}
```

#### ApiKeyUsageLog.cs

```csharp
// File: Domain/Entities/Integration/ApiKeyUsageLog.cs
namespace EnormTech.B2B.Domain.Entities.Integration
{
    public class ApiKeyUsageLog
    {
        public long Id { get; set; }
        public int ApiKeyId { get; set; }
        public string Endpoint { get; set; }
        public string HttpMethod { get; set; }
        public string IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public int ResponseStatusCode { get; set; }
        public int ResponseTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
        
        // Navigation
        public ApiKey ApiKey { get; set; }
        
        public static ApiKeyUsageLog Create(
            int apiKeyId,
            string endpoint,
            string httpMethod,
            string ipAddress,
            string? userAgent,
            int responseStatusCode,
            int responseTimeMs,
            string? errorMessage = null)
        {
            return new ApiKeyUsageLog
            {
                ApiKeyId = apiKeyId,
                Endpoint = endpoint,
                HttpMethod = httpMethod,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                RequestTimestamp = DateTime.UtcNow,
                ResponseStatusCode = responseStatusCode,
                ResponseTimeMs = responseTimeMs,
                ErrorMessage = errorMessage
            };
        }
    }
}
```

### Enums & Constants

#### IntegrationPermissionScopes.cs

```csharp
// File: Domain/Enums/IntegrationPermissionScopes.cs
namespace EnormTech.B2B.Domain.Enums
{
    public static class IntegrationPermissionScopes
    {
        // Product Scopes
        public const string ProductsRead = "products:read";
        public const string ProductsWrite = "products:write";
        
        // Stock Scopes
        public const string StockRead = "stock:read";
        public const string StockWrite = "stock:write";
        
        // Price Scopes
        public const string PricesRead = "prices:read";
        public const string PricesWrite = "prices:write";
        
        // Customer Scopes
        public const string CustomersRead = "customers:read";
        public const string CustomersWrite = "customers:write";
        
        // Order Scopes
        public const string OrdersRead = "orders:read";
        public const string OrdersWrite = "orders:write";
        
        // Invoice Scopes
        public const string InvoicesRead = "invoices:read";
        public const string InvoicesWrite = "invoices:write";
        
        // Webhook Management
        public const string WebhooksManage = "webhooks:manage";
        
        // Wildcards
        public const string All = "*";
        
        private static readonly HashSet<string> ValidScopes = new(StringComparer.OrdinalIgnoreCase)
        {
            ProductsRead, ProductsWrite,
            StockRead, StockWrite,
            PricesRead, PricesWrite,
            CustomersRead, CustomersWrite,
            OrdersRead, OrdersWrite,
            InvoicesRead, InvoicesWrite,
            WebhooksManage,
            All,
            "products:*", "stock:*", "prices:*", 
            "customers:*", "orders:*", "invoices:*"
        };
        
        public static bool IsValidScope(string scope) => ValidScopes.Contains(scope);
        
        public static IEnumerable<string> GetAllScopes() => ValidScopes;
        
        public static IEnumerable<string> GetReadOnlyScopes() => new[]
        {
            ProductsRead, StockRead, PricesRead, 
            CustomersRead, OrdersRead, InvoicesRead
        };
        
        public static IEnumerable<string> GetWriteScopes() => new[]
        {
            ProductsWrite, StockWrite, PricesWrite,
            CustomersWrite, OrdersWrite, InvoicesWrite,
            WebhooksManage
        };
    }
}
```

### Helpers

#### IpAddressHelper.cs

```csharp
// File: Domain/Helpers/IpAddressHelper.cs
using System.Net;

namespace EnormTech.B2B.Domain.Helpers
{
    public static class IpAddressHelper
    {
        public static bool IsValidIpOrCidr(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            
            // Check for CIDR notation
            if (input.Contains('/'))
            {
                var parts = input.Split('/');
                if (parts.Length != 2) return false;
                
                if (!IPAddress.TryParse(parts[0], out _)) return false;
                if (!int.TryParse(parts[1], out var prefix)) return false;
                
                // Valid prefix range for IPv4 (0-32) and IPv6 (0-128)
                return prefix >= 0 && prefix <= 128;
            }
            
            return IPAddress.TryParse(input, out _);
        }
        
        public static bool IsInRange(string ipAddress, string cidr)
        {
            if (!cidr.Contains('/'))
            {
                return ipAddress == cidr;
            }
            
            var parts = cidr.Split('/');
            var network = IPAddress.Parse(parts[0]);
            var prefixLength = int.Parse(parts[1]);
            var address = IPAddress.Parse(ipAddress);
            
            var networkBytes = network.GetAddressBytes();
            var addressBytes = address.GetAddressBytes();
            
            if (networkBytes.Length != addressBytes.Length) return false;
            
            var fullBytes = prefixLength / 8;
            var remainingBits = prefixLength % 8;
            
            for (int i = 0; i < fullBytes; i++)
            {
                if (networkBytes[i] != addressBytes[i]) return false;
            }
            
            if (remainingBits > 0 && fullBytes < networkBytes.Length)
            {
                var mask = (byte)(0xFF << (8 - remainingBits));
                if ((networkBytes[fullBytes] & mask) != (addressBytes[fullBytes] & mask))
                    return false;
            }
            
            return true;
        }
    }
}
```

---

## Application Layer

### DTOs

#### ApiClientDtos.cs

```csharp
// File: Application/DTOs/Integration/ApiClientDtos.cs
namespace EnormTech.B2B.Application.DTOs.Integration
{
    // List/Summary DTO
    public class ApiClientListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public bool IsActive { get; set; }
        public int ActiveKeyCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    // Detail DTO
    public class ApiClientDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public List<ApiKeyListDto> ApiKeys { get; set; } = new();
    }
    
    // Create DTO
    public class CreateApiClientDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }
    
    // Update DTO
    public class UpdateApiClientDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }
}
```

#### ApiKeyDtos.cs

```csharp
// File: Application/DTOs/Integration/ApiKeyDtos.cs
namespace EnormTech.B2B.Application.DTOs.Integration
{
    // List/Summary DTO
    public class ApiKeyListDto
    {
        public int Id { get; set; }
        public string KeyPrefix { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public int RateLimitPerMinute { get; set; }
        public int PermissionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    // Detail DTO
    public class ApiKeyDetailDto
    {
        public int Id { get; set; }
        public int ApiClientId { get; set; }
        public string ApiClientName { get; set; }
        public string KeyPrefix { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public string? LastUsedIp { get; set; }
        public int RateLimitPerMinute { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedBy { get; set; }
        public string? RevocationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public List<string> Permissions { get; set; } = new();
        public List<IpWhitelistDto> IpWhitelist { get; set; } = new();
    }
    
    // Create DTO
    public class CreateApiKeyDto
    {
        public int ApiClientId { get; set; }
        public string Name { get; set; }
        public int RateLimitPerMinute { get; set; } = 500;
        public DateTime? ExpiresAt { get; set; }
        public List<string> Permissions { get; set; } = new();
        public List<string> IpWhitelist { get; set; } = new();
    }
    
    // Create Response (includes plain text key - only shown once)
    public class CreateApiKeyResponseDto
    {
        public int Id { get; set; }
        public string KeyPrefix { get; set; }
        public string PlainTextKey { get; set; } // Only returned on creation!
        public string Name { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<string> Permissions { get; set; } = new();
        public string Warning { get; set; } = "Store this key securely. It will not be shown again.";
    }
    
    // Update DTO
    public class UpdateApiKeyDto
    {
        public string Name { get; set; }
        public int RateLimitPerMinute { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
    
    // Revoke DTO
    public class RevokeApiKeyDto
    {
        public string Reason { get; set; }
    }
    
    // Permission Update DTO
    public class UpdateApiKeyPermissionsDto
    {
        public List<string> Permissions { get; set; } = new();
    }
    
    // IP Whitelist DTO
    public class IpWhitelistDto
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string? Description { get; set; }
    }
    
    // Add IP DTO
    public class AddIpWhitelistDto
    {
        public string IpAddress { get; set; }
        public string? Description { get; set; }
    }
}
```

#### ApiKeyUsageLogDtos.cs

```csharp
// File: Application/DTOs/Integration/ApiKeyUsageLogDtos.cs
namespace EnormTech.B2B.Application.DTOs.Integration
{
    public class ApiKeyUsageLogDto
    {
        public long Id { get; set; }
        public string Endpoint { get; set; }
        public string HttpMethod { get; set; }
        public string IpAddress { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public int ResponseStatusCode { get; set; }
        public int ResponseTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
    }
    
    public class ApiKeyUsageStatsDto
    {
        public int ApiKeyId { get; set; }
        public string KeyPrefix { get; set; }
        public string KeyName { get; set; }
        public long TotalRequests { get; set; }
        public long SuccessfulRequests { get; set; }
        public long FailedRequests { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public DateTime? FirstUsedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public Dictionary<string, long> RequestsByEndpoint { get; set; } = new();
        public Dictionary<int, long> RequestsByStatusCode { get; set; } = new();
    }
    
    public class UsageLogFilterDto
    {
        public int? ApiKeyId { get; set; }
        public int? ApiClientId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Endpoint { get; set; }
        public int? StatusCode { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
```

### Interfaces

#### IApiClientRepository.cs

```csharp
// File: Application/Interfaces/Repositories/IApiClientRepository.cs
namespace EnormTech.B2B.Application.Interfaces.Repositories
{
    public interface IApiClientRepository : IGenericRepository<ApiClient>
    {
        Task<ApiClient?> GetByNameAsync(string name);
        Task<ApiClient?> GetWithKeysAsync(int id);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
        Task<List<ApiClient>> GetActiveClientsAsync();
        Task<int> GetActiveKeyCountAsync(int clientId);
    }
}
```

#### IApiKeyRepository.cs

```csharp
// File: Application/Interfaces/Repositories/IApiKeyRepository.cs
namespace EnormTech.B2B.Application.Interfaces.Repositories
{
    public interface IApiKeyRepository : IGenericRepository<ApiKey>
    {
        Task<ApiKey?> GetByHashAsync(string keyHash);
        Task<ApiKey?> GetByPrefixAsync(string keyPrefix);
        Task<ApiKey?> GetWithDetailsAsync(int id);
        Task<List<ApiKey>> GetByClientIdAsync(int clientId);
        Task<List<ApiKey>> GetActiveKeysAsync();
        Task<List<ApiKey>> GetExpiringKeysAsync(int daysUntilExpiration);
        Task UpdateLastUsedAsync(int keyId, string ipAddress);
    }
}
```

#### IApiKeyUsageLogRepository.cs

```csharp
// File: Application/Interfaces/Repositories/IApiKeyUsageLogRepository.cs
namespace EnormTech.B2B.Application.Interfaces.Repositories
{
    public interface IApiKeyUsageLogRepository
    {
        Task AddAsync(ApiKeyUsageLog log);
        Task AddBatchAsync(IEnumerable<ApiKeyUsageLog> logs);
        Task<PagedResult<ApiKeyUsageLog>> GetLogsAsync(UsageLogFilterDto filter);
        Task<ApiKeyUsageStatsDto> GetStatsAsync(int apiKeyId, DateTime fromDate, DateTime toDate);
        Task<long> GetRequestCountAsync(int apiKeyId, DateTime since);
        Task DeleteOldLogsAsync(DateTime olderThan);
    }
}
```

### Services

#### IApiKeyService.cs

```csharp
// File: Application/Interfaces/Services/IApiKeyService.cs
namespace EnormTech.B2B.Application.Interfaces.Services
{
    public interface IApiKeyService
    {
        // API Client Management
        Task<Result<ApiClientDetailDto>> GetClientAsync(int id);
        Task<Result<PagedResult<ApiClientListDto>>> GetClientsAsync(int page, int pageSize, bool? isActive = null);
        Task<Result<ApiClientDetailDto>> CreateClientAsync(CreateApiClientDto dto, string createdBy);
        Task<Result<ApiClientDetailDto>> UpdateClientAsync(int id, UpdateApiClientDto dto, string updatedBy);
        Task<Result> ActivateClientAsync(int id, string updatedBy);
        Task<Result> DeactivateClientAsync(int id, string updatedBy);
        Task<Result> DeleteClientAsync(int id, string deletedBy);
        
        // API Key Management
        Task<Result<ApiKeyDetailDto>> GetKeyAsync(int id);
        Task<Result<List<ApiKeyListDto>>> GetKeysByClientAsync(int clientId);
        Task<Result<CreateApiKeyResponseDto>> CreateKeyAsync(CreateApiKeyDto dto, string createdBy);
        Task<Result<ApiKeyDetailDto>> UpdateKeyAsync(int id, UpdateApiKeyDto dto, string updatedBy);
        Task<Result> RevokeKeyAsync(int id, RevokeApiKeyDto dto, string revokedBy);
        Task<Result<CreateApiKeyResponseDto>> RotateKeyAsync(int id, string createdBy);
        
        // Permission Management
        Task<Result> UpdatePermissionsAsync(int keyId, UpdateApiKeyPermissionsDto dto);
        Task<Result> AddPermissionAsync(int keyId, string scope);
        Task<Result> RemovePermissionAsync(int keyId, string scope);
        
        // IP Whitelist Management
        Task<Result> AddIpToWhitelistAsync(int keyId, AddIpWhitelistDto dto);
        Task<Result> RemoveIpFromWhitelistAsync(int keyId, int whitelistId);
        
        // Key Validation (for authentication handler)
        Task<ApiKeyValidationResult> ValidateKeyAsync(string plainTextKey, string ipAddress);
        
        // Usage & Stats
        Task<Result<PagedResult<ApiKeyUsageLogDto>>> GetUsageLogsAsync(UsageLogFilterDto filter);
        Task<Result<ApiKeyUsageStatsDto>> GetUsageStatsAsync(int keyId, DateTime fromDate, DateTime toDate);
    }
    
    public class ApiKeyValidationResult
    {
        public bool IsValid { get; set; }
        public int? ApiKeyId { get; set; }
        public int? ApiClientId { get; set; }
        public string? ClientName { get; set; }
        public List<string> Permissions { get; set; } = new();
        public int RateLimitPerMinute { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        
        public static ApiKeyValidationResult Success(ApiKey key, ApiClient client) => new()
        {
            IsValid = true,
            ApiKeyId = key.Id,
            ApiClientId = client.Id,
            ClientName = client.Name,
            Permissions = key.Permissions.Select(p => p.Scope).ToList(),
            RateLimitPerMinute = key.RateLimitPerMinute
        };
        
        public static ApiKeyValidationResult Failed(string errorCode, string message) => new()
        {
            IsValid = false,
            ErrorCode = errorCode,
            ErrorMessage = message
        };
    }
}
```

#### IApiKeyGenerator.cs

```csharp
// File: Application/Interfaces/Services/IApiKeyGenerator.cs
namespace EnormTech.B2B.Application.Interfaces.Services
{
    public interface IApiKeyGenerator
    {
        /// <summary>
        /// Generates a new API key with prefix
        /// </summary>
        /// <returns>Tuple of (PlainTextKey, KeyHash, KeyPrefix)</returns>
        (string PlainTextKey, string KeyHash, string KeyPrefix) GenerateKey();
        
        /// <summary>
        /// Hashes a plain text key for storage
        /// </summary>
        string HashKey(string plainTextKey);
        
        /// <summary>
        /// Verifies a plain text key against a hash
        /// </summary>
        bool VerifyKey(string plainTextKey, string keyHash);
    }
}
```

### Validators

#### CreateApiClientValidator.cs

```csharp
// File: Application/Validators/Integration/CreateApiClientValidator.cs
using FluentValidation;

namespace EnormTech.B2B.Application.Validators.Integration
{
    public class CreateApiClientValidator : AbstractValidator<CreateApiClientDto>
    {
        public CreateApiClientValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Client name is required")
                .MaximumLength(100).WithMessage("Client name cannot exceed 100 characters");
            
            RuleFor(x => x.ContactEmail)
                .NotEmpty().WithMessage("Contact email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");
            
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
            
            RuleFor(x => x.ContactPhone)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
                .Matches(@"^[\d\s\+\-\(\)]+$").When(x => !string.IsNullOrEmpty(x.ContactPhone))
                .WithMessage("Invalid phone number format");
        }
    }
}
```

#### CreateApiKeyValidator.cs

```csharp
// File: Application/Validators/Integration/CreateApiKeyValidator.cs
using FluentValidation;

namespace EnormTech.B2B.Application.Validators.Integration
{
    public class CreateApiKeyValidator : AbstractValidator<CreateApiKeyDto>
    {
        public CreateApiKeyValidator()
        {
            RuleFor(x => x.ApiClientId)
                .GreaterThan(0).WithMessage("Valid API Client ID is required");
            
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Key name is required")
                .MaximumLength(100).WithMessage("Key name cannot exceed 100 characters");
            
            RuleFor(x => x.RateLimitPerMinute)
                .GreaterThan(0).WithMessage("Rate limit must be positive")
                .LessThanOrEqualTo(10000).WithMessage("Rate limit cannot exceed 10000 per minute");
            
            RuleFor(x => x.ExpiresAt)
                .GreaterThan(DateTime.UtcNow).When(x => x.ExpiresAt.HasValue)
                .WithMessage("Expiration date must be in the future");
            
            RuleFor(x => x.Permissions)
                .NotEmpty().WithMessage("At least one permission is required");
            
            RuleForEach(x => x.Permissions)
                .Must(IntegrationPermissionScopes.IsValidScope)
                .WithMessage(scope => $"Invalid permission scope: {scope}");
            
            RuleForEach(x => x.IpWhitelist)
                .Must(IpAddressHelper.IsValidIpOrCidr)
                .WithMessage(ip => $"Invalid IP address or CIDR: {ip}");
        }
    }
}
```

### AutoMapper Profile

```csharp
// File: Application/Mapping/IntegrationMappingProfile.cs
using AutoMapper;

namespace EnormTech.B2B.Application.Mapping
{
    public class IntegrationMappingProfile : Profile
    {
        public IntegrationMappingProfile()
        {
            // ApiClient mappings
            CreateMap<ApiClient, ApiClientListDto>()
                .ForMember(d => d.ActiveKeyCount, 
                    o => o.MapFrom(s => s.ApiKeys.Count(k => k.IsActive && !k.IsExpired())));
            
            CreateMap<ApiClient, ApiClientDetailDto>()
                .ForMember(d => d.ApiKeys, o => o.MapFrom(s => s.ApiKeys));
            
            // ApiKey mappings
            CreateMap<ApiKey, ApiKeyListDto>()
                .ForMember(d => d.IsExpired, o => o.MapFrom(s => s.IsExpired()))
                .ForMember(d => d.IsRevoked, o => o.MapFrom(s => s.IsRevoked()))
                .ForMember(d => d.PermissionCount, o => o.MapFrom(s => s.Permissions.Count));
            
            CreateMap<ApiKey, ApiKeyDetailDto>()
                .ForMember(d => d.ApiClientName, o => o.MapFrom(s => s.ApiClient.Name))
                .ForMember(d => d.IsExpired, o => o.MapFrom(s => s.IsExpired()))
                .ForMember(d => d.IsRevoked, o => o.MapFrom(s => s.IsRevoked()))
                .ForMember(d => d.Permissions, o => o.MapFrom(s => s.Permissions.Select(p => p.Scope)))
                .ForMember(d => d.IpWhitelist, o => o.MapFrom(s => s.IpWhitelist));
            
            // IP Whitelist mapping
            CreateMap<ApiKeyIpWhitelist, IpWhitelistDto>();
            
            // Usage Log mapping
            CreateMap<ApiKeyUsageLog, ApiKeyUsageLogDto>();
        }
    }
}
```

---

## Infrastructure Layer

### Entity Configurations

#### ApiClientConfiguration.cs

```csharp
// File: Infrastructure/Data/Configurations/Integration/ApiClientConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnormTech.B2B.Infrastructure.Data.Configurations.Integration
{
    public class ApiClientConfiguration : IEntityTypeConfiguration<ApiClient>
    {
        public void Configure(EntityTypeBuilder<ApiClient> builder)
        {
            builder.ToTable("ApiClients");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(x => x.Description)
                .HasMaxLength(500);
            
            builder.Property(x => x.ContactEmail)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(x => x.ContactPhone)
                .HasMaxLength(20);
            
            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            // Indexes
            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasFilter("IsDeleted = 0");
            
            builder.HasIndex(x => x.IsActive)
                .HasFilter("IsDeleted = 0");
            
            // Relationships
            builder.HasMany(x => x.ApiKeys)
                .WithOne(x => x.ApiClient)
                .HasForeignKey(x => x.ApiClientId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Query Filter (soft delete)
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
```

#### ApiKeyConfiguration.cs

```csharp
// File: Infrastructure/Data/Configurations/Integration/ApiKeyConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnormTech.B2B.Infrastructure.Data.Configurations.Integration
{
    public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
    {
        public void Configure(EntityTypeBuilder<ApiKey> builder)
        {
            builder.ToTable("ApiKeys");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.KeyHash)
                .IsRequired()
                .HasMaxLength(256);
            
            builder.Property(x => x.KeyPrefix)
                .IsRequired()
                .HasMaxLength(8);
            
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(x => x.RateLimitPerMinute)
                .IsRequired()
                .HasDefaultValue(500);
            
            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(x => x.LastUsedIp)
                .HasMaxLength(45);
            
            builder.Property(x => x.RevokedBy)
                .HasMaxLength(100);
            
            builder.Property(x => x.RevocationReason)
                .HasMaxLength(500);
            
            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100);
            
            // Indexes
            builder.HasIndex(x => x.KeyHash);
            builder.HasIndex(x => x.KeyPrefix);
            builder.HasIndex(x => x.ApiClientId);
            builder.HasIndex(x => new { x.IsActive, x.ExpiresAt });
            
            // Relationships
            builder.HasMany(x => x.Permissions)
                .WithOne(x => x.ApiKey)
                .HasForeignKey(x => x.ApiKeyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasMany(x => x.IpWhitelist)
                .WithOne(x => x.ApiKey)
                .HasForeignKey(x => x.ApiKeyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

#### ApiKeyPermissionConfiguration.cs

```csharp
// File: Infrastructure/Data/Configurations/Integration/ApiKeyPermissionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnormTech.B2B.Infrastructure.Data.Configurations.Integration
{
    public class ApiKeyPermissionConfiguration : IEntityTypeConfiguration<ApiKeyPermission>
    {
        public void Configure(EntityTypeBuilder<ApiKeyPermission> builder)
        {
            builder.ToTable("ApiKeyPermissions");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Scope)
                .IsRequired()
                .HasMaxLength(100);
            
            // Indexes
            builder.HasIndex(x => x.ApiKeyId);
            builder.HasIndex(x => new { x.ApiKeyId, x.Scope }).IsUnique();
        }
    }
}
```

#### ApiKeyIpWhitelistConfiguration.cs

```csharp
// File: Infrastructure/Data/Configurations/Integration/ApiKeyIpWhitelistConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnormTech.B2B.Infrastructure.Data.Configurations.Integration
{
    public class ApiKeyIpWhitelistConfiguration : IEntityTypeConfiguration<ApiKeyIpWhitelist>
    {
        public void Configure(EntityTypeBuilder<ApiKeyIpWhitelist> builder)
        {
            builder.ToTable("ApiKeyIpWhitelist");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.IpAddress)
                .IsRequired()
                .HasMaxLength(45);
            
            builder.Property(x => x.Description)
                .HasMaxLength(200);
            
            // Indexes
            builder.HasIndex(x => x.ApiKeyId);
        }
    }
}
```

#### ApiKeyUsageLogConfiguration.cs

```csharp
// File: Infrastructure/Data/Configurations/Integration/ApiKeyUsageLogConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnormTech.B2B.Infrastructure.Data.Configurations.Integration
{
    public class ApiKeyUsageLogConfiguration : IEntityTypeConfiguration<ApiKeyUsageLog>
    {
        public void Configure(EntityTypeBuilder<ApiKeyUsageLog> builder)
        {
            builder.ToTable("ApiKeyUsageLogs");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Endpoint)
                .IsRequired()
                .HasMaxLength(500);
            
            builder.Property(x => x.HttpMethod)
                .IsRequired()
                .HasMaxLength(10);
            
            builder.Property(x => x.IpAddress)
                .IsRequired()
                .HasMaxLength(45);
            
            builder.Property(x => x.UserAgent)
                .HasMaxLength(500);
            
            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(1000);
            
            // Indexes
            builder.HasIndex(x => x.ApiKeyId);
            builder.HasIndex(x => x.RequestTimestamp);
            builder.HasIndex(x => new { x.ApiKeyId, x.RequestTimestamp });
            
            // No navigation to ApiKey (for performance - this is a log table)
            builder.HasOne(x => x.ApiKey)
                .WithMany()
                .HasForeignKey(x => x.ApiKeyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

### Repository Implementations

#### ApiClientRepository.cs

```csharp
// File: Infrastructure/Repositories/Integration/ApiClientRepository.cs
using Microsoft.EntityFrameworkCore;

namespace EnormTech.B2B.Infrastructure.Repositories.Integration
{
    public class ApiClientRepository : GenericRepository<ApiClient>, IApiClientRepository
    {
        public ApiClientRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<ApiClient?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Name == name);
        }
        
        public async Task<ApiClient?> GetWithKeysAsync(int id)
        {
            return await _dbSet
                .Include(x => x.ApiKeys)
                    .ThenInclude(k => k.Permissions)
                .Include(x => x.ApiKeys)
                    .ThenInclude(k => k.IpWhitelist)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        
        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(x => x.Name == name);
            
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }
            
            return !await query.AnyAsync();
        }
        
        public async Task<List<ApiClient>> GetActiveClientsAsync()
        {
            return await _dbSet
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
        
        public async Task<int> GetActiveKeyCountAsync(int clientId)
        {
            return await _context.Set<ApiKey>()
                .Where(k => k.ApiClientId == clientId && k.IsActive && 
                       (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow))
                .CountAsync();
        }
    }
}
```

#### ApiKeyRepository.cs

```csharp
// File: Infrastructure/Repositories/Integration/ApiKeyRepository.cs
using Microsoft.EntityFrameworkCore;

namespace EnormTech.B2B.Infrastructure.Repositories.Integration
{
    public class ApiKeyRepository : GenericRepository<ApiKey>, IApiKeyRepository
    {
        public ApiKeyRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<ApiKey?> GetByHashAsync(string keyHash)
        {
            return await _dbSet
                .Include(x => x.ApiClient)
                .Include(x => x.Permissions)
                .Include(x => x.IpWhitelist)
                .FirstOrDefaultAsync(x => x.KeyHash == keyHash);
        }
        
        public async Task<ApiKey?> GetByPrefixAsync(string keyPrefix)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.KeyPrefix == keyPrefix);
        }
        
        public async Task<ApiKey?> GetWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(x => x.ApiClient)
                .Include(x => x.Permissions)
                .Include(x => x.IpWhitelist)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        
        public async Task<List<ApiKey>> GetByClientIdAsync(int clientId)
        {
            return await _dbSet
                .Where(x => x.ApiClientId == clientId)
                .Include(x => x.Permissions)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<List<ApiKey>> GetActiveKeysAsync()
        {
            return await _dbSet
                .Where(x => x.IsActive && (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow))
                .Include(x => x.ApiClient)
                .ToListAsync();
        }
        
        public async Task<List<ApiKey>> GetExpiringKeysAsync(int daysUntilExpiration)
        {
            var expirationThreshold = DateTime.UtcNow.AddDays(daysUntilExpiration);
            
            return await _dbSet
                .Where(x => x.IsActive && 
                       x.ExpiresAt != null && 
                       x.ExpiresAt <= expirationThreshold &&
                       x.ExpiresAt > DateTime.UtcNow)
                .Include(x => x.ApiClient)
                .OrderBy(x => x.ExpiresAt)
                .ToListAsync();
        }
        
        public async Task UpdateLastUsedAsync(int keyId, string ipAddress)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE ApiKeys SET LastUsedAt = {DateTime.UtcNow}, LastUsedIp = {ipAddress} WHERE Id = {keyId}");
        }
    }
}
```

#### ApiKeyUsageLogRepository.cs

```csharp
// File: Infrastructure/Repositories/Integration/ApiKeyUsageLogRepository.cs
using Microsoft.EntityFrameworkCore;

namespace EnormTech.B2B.Infrastructure.Repositories.Integration
{
    public class ApiKeyUsageLogRepository : IApiKeyUsageLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<ApiKeyUsageLog> _dbSet;
        
        public ApiKeyUsageLogRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<ApiKeyUsageLog>();
        }
        
        public async Task AddAsync(ApiKeyUsageLog log)
        {
            await _dbSet.AddAsync(log);
            await _context.SaveChangesAsync();
        }
        
        public async Task AddBatchAsync(IEnumerable<ApiKeyUsageLog> logs)
        {
            await _dbSet.AddRangeAsync(logs);
            await _context.SaveChangesAsync();
        }
        
        public async Task<PagedResult<ApiKeyUsageLog>> GetLogsAsync(UsageLogFilterDto filter)
        {
            var query = _dbSet.AsQueryable();
            
            if (filter.ApiKeyId.HasValue)
                query = query.Where(x => x.ApiKeyId == filter.ApiKeyId.Value);
            
            if (filter.ApiClientId.HasValue)
                query = query.Where(x => x.ApiKey.ApiClientId == filter.ApiClientId.Value);
            
            if (filter.FromDate.HasValue)
                query = query.Where(x => x.RequestTimestamp >= filter.FromDate.Value);
            
            if (filter.ToDate.HasValue)
                query = query.Where(x => x.RequestTimestamp <= filter.ToDate.Value);
            
            if (!string.IsNullOrEmpty(filter.Endpoint))
                query = query.Where(x => x.Endpoint.Contains(filter.Endpoint));
            
            if (filter.StatusCode.HasValue)
                query = query.Where(x => x.ResponseStatusCode == filter.StatusCode.Value);
            
            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderByDescending(x => x.RequestTimestamp)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
            
            return new PagedResult<ApiKeyUsageLog>(items, totalCount, filter.Page, filter.PageSize);
        }
        
        public async Task<ApiKeyUsageStatsDto> GetStatsAsync(int apiKeyId, DateTime fromDate, DateTime toDate)
        {
            var logs = await _dbSet
                .Where(x => x.ApiKeyId == apiKeyId && 
                       x.RequestTimestamp >= fromDate && 
                       x.RequestTimestamp <= toDate)
                .ToListAsync();
            
            var apiKey = await _context.Set<ApiKey>()
                .FirstOrDefaultAsync(x => x.Id == apiKeyId);
            
            return new ApiKeyUsageStatsDto
            {
                ApiKeyId = apiKeyId,
                KeyPrefix = apiKey?.KeyPrefix ?? "",
                KeyName = apiKey?.Name ?? "",
                TotalRequests = logs.Count,
                SuccessfulRequests = logs.Count(x => x.ResponseStatusCode >= 200 && x.ResponseStatusCode < 400),
                FailedRequests = logs.Count(x => x.ResponseStatusCode >= 400),
                AverageResponseTimeMs = logs.Any() ? logs.Average(x => x.ResponseTimeMs) : 0,
                FirstUsedAt = logs.MinBy(x => x.RequestTimestamp)?.RequestTimestamp,
                LastUsedAt = logs.MaxBy(x => x.RequestTimestamp)?.RequestTimestamp,
                RequestsByEndpoint = logs.GroupBy(x => x.Endpoint)
                    .ToDictionary(g => g.Key, g => (long)g.Count()),
                RequestsByStatusCode = logs.GroupBy(x => x.ResponseStatusCode)
                    .ToDictionary(g => g.Key, g => (long)g.Count())
            };
        }
        
        public async Task<long> GetRequestCountAsync(int apiKeyId, DateTime since)
        {
            return await _dbSet
                .Where(x => x.ApiKeyId == apiKeyId && x.RequestTimestamp >= since)
                .LongCountAsync();
        }
        
        public async Task DeleteOldLogsAsync(DateTime olderThan)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM ApiKeyUsageLogs WHERE RequestTimestamp < {olderThan}");
        }
    }
}
```

### Service Implementations

#### ApiKeyGenerator.cs

```csharp
// File: Infrastructure/Services/Integration/ApiKeyGenerator.cs
using System.Security.Cryptography;
using System.Text;

namespace EnormTech.B2B.Infrastructure.Services.Integration
{
    public class ApiKeyGenerator : IApiKeyGenerator
    {
        private const string KeyPrefix = "ek_live_";
        private const int KeyLength = 32; // 256 bits
        
        public (string PlainTextKey, string KeyHash, string KeyPrefix) GenerateKey()
        {
            // Generate random bytes
            var randomBytes = RandomNumberGenerator.GetBytes(KeyLength);
            var randomPart = Convert.ToBase64String(randomBytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, 40); // Take first 40 chars for readability
            
            var plainTextKey = $"{KeyPrefix}{randomPart}";
            var keyHash = HashKey(plainTextKey);
            
            return (plainTextKey, keyHash, KeyPrefix);
        }
        
        public string HashKey(string plainTextKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(plainTextKey);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        
        public bool VerifyKey(string plainTextKey, string keyHash)
        {
            var computedHash = HashKey(plainTextKey);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHash),
                Encoding.UTF8.GetBytes(keyHash));
        }
    }
}
```

#### ApiKeyService.cs

```csharp
// File: Infrastructure/Services/Integration/ApiKeyService.cs
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace EnormTech.B2B.Infrastructure.Services.Integration
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApiKeyGenerator _keyGenerator;
        private readonly IMapper _mapper;
        private readonly ILogger<ApiKeyService> _logger;
        
        public ApiKeyService(
            IUnitOfWork unitOfWork,
            IApiKeyGenerator keyGenerator,
            IMapper mapper,
            ILogger<ApiKeyService> logger)
        {
            _unitOfWork = unitOfWork;
            _keyGenerator = keyGenerator;
            _mapper = mapper;
            _logger = logger;
        }
        
        #region API Client Management
        
        public async Task<Result<ApiClientDetailDto>> GetClientAsync(int id)
        {
            var client = await _unitOfWork.ApiClients.GetWithKeysAsync(id);
            if (client == null)
                return Result<ApiClientDetailDto>.Failure("Client not found");
            
            return Result<ApiClientDetailDto>.Success(_mapper.Map<ApiClientDetailDto>(client));
        }
        
        public async Task<Result<PagedResult<ApiClientListDto>>> GetClientsAsync(
            int page, int pageSize, bool? isActive = null)
        {
            var result = await _unitOfWork.ApiClients.GetPagedAsync(
                filter: isActive.HasValue ? x => x.IsActive == isActive.Value : null,
                orderBy: q => q.OrderBy(x => x.Name),
                page: page,
                pageSize: pageSize);
            
            var dtos = _mapper.Map<List<ApiClientListDto>>(result.Items);
            
            return Result<PagedResult<ApiClientListDto>>.Success(
                new PagedResult<ApiClientListDto>(dtos, result.TotalCount, page, pageSize));
        }
        
        public async Task<Result<ApiClientDetailDto>> CreateClientAsync(
            CreateApiClientDto dto, string createdBy)
        {
            // Check uniqueness
            if (!await _unitOfWork.ApiClients.IsNameUniqueAsync(dto.Name))
                return Result<ApiClientDetailDto>.Failure("Client name already exists");
            
            var client = ApiClient.Create(
                dto.Name,
                dto.ContactEmail,
                dto.Description,
                dto.ContactPhone);
            
            client.CreatedBy = createdBy;
            client.CreatedAt = DateTime.UtcNow;
            
            await _unitOfWork.ApiClients.AddAsync(client);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("API Client created: {ClientId} - {ClientName} by {CreatedBy}",
                client.Id, client.Name, createdBy);
            
            return Result<ApiClientDetailDto>.Success(_mapper.Map<ApiClientDetailDto>(client));
        }
        
        public async Task<Result<ApiClientDetailDto>> UpdateClientAsync(
            int id, UpdateApiClientDto dto, string updatedBy)
        {
            var client = await _unitOfWork.ApiClients.GetByIdAsync(id);
            if (client == null)
                return Result<ApiClientDetailDto>.Failure("Client not found");
            
            if (!await _unitOfWork.ApiClients.IsNameUniqueAsync(dto.Name, id))
                return Result<ApiClientDetailDto>.Failure("Client name already exists");
            
            client.Update(dto.Name, dto.ContactEmail, dto.Description, dto.ContactPhone);
            client.UpdatedBy = updatedBy;
            client.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.ApiClients.Update(client);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("API Client updated: {ClientId} by {UpdatedBy}", id, updatedBy);
            
            return await GetClientAsync(id);
        }
        
        public async Task<Result> ActivateClientAsync(int id, string updatedBy)
        {
            var client = await _unitOfWork.ApiClients.GetByIdAsync(id);
            if (client == null)
                return Result.Failure("Client not found");
            
            client.Activate();
            client.UpdatedBy = updatedBy;
            client.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.ApiClients.Update(client);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("API Client activated: {ClientId} by {UpdatedBy}", id, updatedBy);
            
            return Result.Success();
        }
        
        public async Task<Result> DeactivateClientAsync(int id, string updatedBy)
        {
            var client = await _unitOfWork.ApiClients.GetWithKeysAsync(id);
            if (client == null)
                return Result.Failure("Client not found");
            
            client.Deactivate();
            client.UpdatedBy = updatedBy;
            client.UpdatedAt = DateTime.UtcNow;
            
            _unitOfWork.ApiClients.Update(client);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("API Client deactivated: {ClientId} by {UpdatedBy}", id, updatedBy);
            
            return Result.Success();
        }
        
        public async Task<Result> DeleteClientAsync(int id, string deletedBy)
        {
            var client = await _unitOfWork.ApiClients.GetWithKeysAsync(id);
            if (client == null)
                return Result.Failure("Client not found");
            
            // Soft delete
            client.IsDeleted = true;
            client.DeletedAt = DateTime.UtcNow;
            client.DeletedBy = deletedBy;
            client.Deactivate();
            
            _unitOfWork.ApiClients.Update(client);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("API Client deleted: {ClientId} by {DeletedBy}", id, deletedBy);
            
            return Result.Success();
        }
        
        #endregion
        
        #region API Key Management
        
        public async Task<Result<ApiKeyDetailDto>> GetKeyAsync(int id)
        {
            var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(id);
            if (key == null)
                return Result<ApiKeyDetailDto>.Failure("API Key not found");
            
            return Result<ApiKeyDetailDto>.Success(_mapper.Map<ApiKeyDetailDto>(key));
        }
        
        public async Task<Result<List<ApiKeyListDto>>> GetKeysByClientAsync(int clientId)
        {
            var keys = await _unitOfWork.ApiKeys.GetByClientIdAsync(clientId);
            return Result<List<ApiKeyListDto>>.Success(_mapper.Map<List<ApiKeyListDto>>(keys));
        }
        
        public async Task<Result<CreateApiKeyResponseDto>> CreateKeyAsync(
            CreateApiKeyDto dto, string createdBy)
        {
            var client = await _unitOfWork.ApiClients.GetByIdAsync(dto.ApiClientId);
            if (client == null)
                return Result<CreateApiKeyResponseDto>.Failure("API Client not found");
            
            if (!client.IsActive)
                return Result<CreateApiKeyResponseDto>.Failure("Cannot create key for inactive client");
            
            // Generate key
            var (plainTextKey, keyHash, keyPrefix) = _keyGenerator.GenerateKey();
            
            // Create API key entity
            var apiKey = ApiKey.Create(
                dto.ApiClientId,
                keyHash,
                keyPrefix,
                dto.Name,
                dto.RateLimitPerMinute,
                dto.ExpiresAt,
                createdBy);
            
            // Add permissions
            foreach (var scope in dto.Permissions)
            {
                apiKey.AddPermission(scope);
            }
            
            // Add IP whitelist
            foreach (var ip in dto.IpWhitelist)
            {
                apiKey.AddIpToWhitelist(ip);
            }
            
            await _unitOfWork.ApiKeys.AddAsync(apiKey);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "API Key created: {KeyId} for client {ClientId} by {CreatedBy}",
                apiKey.Id, dto.ApiClientId, createdBy);
            
            return Result<CreateApiKeyResponseDto>.Success(new CreateApiKeyResponseDto
            {
                Id = apiKey.Id,
                KeyPrefix = keyPrefix,
                PlainTextKey = plainTextKey, // Only returned once!
                Name = apiKey.Name,
                ExpiresAt = apiKey.ExpiresAt,
                Permissions = dto.Permissions
            });
        }
        
        public async Task<Result<ApiKeyDetailDto>> UpdateKeyAsync(
            int id, UpdateApiKeyDto dto, string updatedBy)
        {
            var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(id);
            if (key == null)
                return Result<ApiKeyDetailDto>.Failure("API Key not found");
            
            if (key.IsRevoked())
                return Result<ApiKeyDetailDto>.Failure("Cannot update revoked key");
            
            // Update properties (name, rate limit, expiration only - not the key itself)
            // For key rotation, use RotateKeyAsync
            key.UpdateRateLimit(dto.RateLimitPerMinute);
            key.UpdateExpiration(dto.ExpiresAt);
            // Name update would require adding a method to the entity
            
            _unitOfWork.ApiKeys.Update(key);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("API Key updated: {KeyId} by {UpdatedBy}", id, updatedBy);
            
            return await GetKeyAsync(id);
        }
        
        public async Task<Result> RevokeKeyAsync(int id, RevokeApiKeyDto dto, string revokedBy)
        {
            var key = await _unitOfWork.ApiKeys.GetByIdAsync(id);
            if (key == null)
                return Result.Failure("API Key not found");
            
            if (key.IsRevoked())
                return Result.Failure("Key is already revoked");
            
            key.Revoke(dto.Reason, revokedBy);
            
            _unitOfWork.ApiKeys.Update(key);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogWarning("API Key revoked: {KeyId} by {RevokedBy}. Reason: {Reason}",
                id, revokedBy, dto.Reason);
            
            return Result.Success();
        }
        
        public async Task<Result<CreateApiKeyResponseDto>> RotateKeyAsync(int id, string createdBy)
        {
            var oldKey = await _unitOfWork.ApiKeys.GetWithDetailsAsync(id);
            if (oldKey == null)
                return Result<CreateApiKeyResponseDto>.Failure("API Key not found");
            
            // Create new key with same settings
            var createDto = new CreateApiKeyDto
            {
                ApiClientId = oldKey.ApiClientId,
                Name = $"{oldKey.Name} (Rotated)",
                RateLimitPerMinute = oldKey.RateLimitPerMinute,
                ExpiresAt = oldKey.ExpiresAt,
                Permissions = oldKey.Permissions.Select(p => p.Scope).ToList(),
                IpWhitelist = oldKey.IpWhitelist.Select(ip => ip.IpAddress).ToList()
            };
            
            var newKeyResult = await CreateKeyAsync(createDto, createdBy);
            
            if (!newKeyResult.IsSuccess)
                return newKeyResult;
            
            // Revoke old key
            oldKey.Revoke("Key rotation", createdBy);
            _unitOfWork.ApiKeys.Update(oldKey);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("API Key rotated: Old {OldKeyId} -> New {NewKeyId} by {CreatedBy}",
                id, newKeyResult.Data.Id, createdBy);
            
            return newKeyResult;
        }
        
        #endregion
        
        #region Permission Management
        
        public async Task<Result> UpdatePermissionsAsync(int keyId, UpdateApiKeyPermissionsDto dto)
        {
            var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(keyId);
            if (key == null)
                return Result.Failure("API Key not found");
            
            if (key.IsRevoked())
                return Result.Failure("Cannot update permissions on revoked key");
            
            // Remove existing permissions not in new list
            var toRemove = key.Permissions
                .Where(p => !dto.Permissions.Contains(p.Scope))
                .Select(p => p.Scope)
                .ToList();
            
            foreach (var scope in toRemove)
            {
                key.RemovePermission(scope);
            }
            
            // Add new permissions
            foreach (var scope in dto.Permissions)
            {
                if (!key.HasPermission(scope))
                {
                    key.AddPermission(scope);
                }
            }
            
            _unitOfWork.ApiKeys.Update(key);
            await _unitOfWork.SaveChangesAsync();
            
            return Result.Success();
        }
        
        public async Task<Result> AddPermissionAsync(int keyId, string scope)
        {
            var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(keyId);
            if (key == null)
                return Result.Failure("API Key not found");
            
            key.AddPermission(scope);
            
            _unitOfWork.ApiKeys.Update(key);
            await _unitOfWork.SaveChangesAsync();
            
            return Result.Success();
        }
        
        public async Task<Result> RemovePermissionAsync(int keyId, string scope)
        {
            var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(keyId);
            if (key == null)
                return Result.Failure("API Key not found");
            
            key.RemovePermission(scope);
            
            _unitOfWork.ApiKeys.Update(key);
            await _unitOfWork.SaveChangesAsync();
            
            return Result.Success();
        }
        
        #endregion
        
        #region IP Whitelist Management
        
        public async Task<Result> AddIpToWhitelistAsync(int keyId, AddIpWhitelistDto dto)
        {
            var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(keyId);
            if (key == null)
                return Result.Failure("API Key not found");
            
            key.AddIpToWhitelist(dto.IpAddress, dto.Description);
            
            _unitOfWork.ApiKeys.Update(key);
            await _unitOfWork.SaveChangesAsync();
            
            return Result.Success();
        }
        
        public async Task<Result> RemoveIpFromWhitelistAsync(int keyId, int whitelistId)
        {
            var key = await _unitOfWork.ApiKeys.GetWithDetailsAsync(keyId);
            if (key == null)
                return Result.Failure("API Key not found");
            
            var ip = key.IpWhitelist.FirstOrDefault(x => x.Id == whitelistId);
            if (ip != null)
            {
                key.RemoveIpFromWhitelist(ip.IpAddress);
                _unitOfWork.ApiKeys.Update(key);
                await _unitOfWork.SaveChangesAsync();
            }
            
            return Result.Success();
        }
        
        #endregion
        
        #region Key Validation
        
        public async Task<ApiKeyValidationResult> ValidateKeyAsync(string plainTextKey, string ipAddress)
        {
            if (string.IsNullOrEmpty(plainTextKey))
                return ApiKeyValidationResult.Failed("MISSING_KEY", "API key is required");
            
            // Hash the provided key
            var keyHash = _keyGenerator.HashKey(plainTextKey);
            
            // Find key by hash
            var apiKey = await _unitOfWork.ApiKeys.GetByHashAsync(keyHash);
            
            if (apiKey == null)
                return ApiKeyValidationResult.Failed("INVALID_KEY", "Invalid API key");
            
            // Check if client is active
            if (!apiKey.ApiClient.IsActive)
                return ApiKeyValidationResult.Failed("CLIENT_INACTIVE", "API client is inactive");
            
            // Check if key is valid
            if (!apiKey.IsValid())
            {
                if (apiKey.IsRevoked())
                    return ApiKeyValidationResult.Failed("KEY_REVOKED", "API key has been revoked");
                
                if (apiKey.IsExpired())
                    return ApiKeyValidationResult.Failed("KEY_EXPIRED", "API key has expired");
                
                return ApiKeyValidationResult.Failed("KEY_INACTIVE", "API key is inactive");
            }
            
            // Check IP whitelist
            if (!apiKey.IsIpAllowed(ipAddress))
                return ApiKeyValidationResult.Failed("IP_NOT_ALLOWED", 
                    $"IP address {ipAddress} is not in the whitelist");
            
            // Update last used (fire and forget - don't await)
            _ = _unitOfWork.ApiKeys.UpdateLastUsedAsync(apiKey.Id, ipAddress);
            
            return ApiKeyValidationResult.Success(apiKey, apiKey.ApiClient);
        }
        
        #endregion
        
        #region Usage & Stats
        
        public async Task<Result<PagedResult<ApiKeyUsageLogDto>>> GetUsageLogsAsync(UsageLogFilterDto filter)
        {
            var result = await _unitOfWork.ApiKeyUsageLogs.GetLogsAsync(filter);
            var dtos = _mapper.Map<List<ApiKeyUsageLogDto>>(result.Items);
            
            return Result<PagedResult<ApiKeyUsageLogDto>>.Success(
                new PagedResult<ApiKeyUsageLogDto>(dtos, result.TotalCount, filter.Page, filter.PageSize));
        }
        
        public async Task<Result<ApiKeyUsageStatsDto>> GetUsageStatsAsync(
            int keyId, DateTime fromDate, DateTime toDate)
        {
            var stats = await _unitOfWork.ApiKeyUsageLogs.GetStatsAsync(keyId, fromDate, toDate);
            return Result<ApiKeyUsageStatsDto>.Success(stats);
        }
        
        #endregion
    }
}
```

---

## API Layer

### Admin Controllers

#### ApiClientsController.cs

```csharp
// File: API/Controllers/Admin/ApiClientsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnormTech.B2B.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/integration/clients")]
    [Authorize(Policy = "integration.manage")]
    public class ApiClientsController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ILogger<ApiClientsController> _logger;
        
        public ApiClientsController(
            IApiKeyService apiKeyService,
            ILogger<ApiClientsController> logger)
        {
            _apiKeyService = apiKeyService;
            _logger = logger;
        }
        
        /// <summary>
        /// Get all API clients with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ApiClientListDto>), 200)]
        public async Task<IActionResult> GetClients(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isActive = null)
        {
            var result = await _apiKeyService.GetClientsAsync(page, pageSize, isActive);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Get API client details
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiClientDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetClient(int id)
        {
            var result = await _apiKeyService.GetClientAsync(id);
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
        }
        
        /// <summary>
        /// Create new API client
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiClientDetailDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateClient([FromBody] CreateApiClientDto dto)
        {
            var createdBy = User.Identity?.Name ?? "System";
            var result = await _apiKeyService.CreateClientAsync(dto, createdBy);
            
            return result.IsSuccess 
                ? CreatedAtAction(nameof(GetClient), new { id = result.Data.Id }, result.Data)
                : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Update API client
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiClientDetailDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateApiClientDto dto)
        {
            var updatedBy = User.Identity?.Name ?? "System";
            var result = await _apiKeyService.UpdateClientAsync(id, dto, updatedBy);
            
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Activate API client
        /// </summary>
        [HttpPost("{id}/activate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ActivateClient(int id)
        {
            var updatedBy = User.Identity?.Name ?? "System";
            var result = await _apiKeyService.ActivateClientAsync(id, updatedBy);
            
            return result.IsSuccess ? NoContent() : NotFound(result.Error);
        }
        
        /// <summary>
        /// Deactivate API client (also deactivates all keys)
        /// </summary>
        [HttpPost("{id}/deactivate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeactivateClient(int id)
        {
            var updatedBy = User.Identity?.Name ?? "System";
            var result = await _apiKeyService.DeactivateClientAsync(id, updatedBy);
            
            return result.IsSuccess ? NoContent() : NotFound(result.Error);
        }
        
        /// <summary>
        /// Delete API client (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var deletedBy = User.Identity?.Name ?? "System";
            var result = await _apiKeyService.DeleteClientAsync(id, deletedBy);
            
            return result.IsSuccess ? NoContent() : NotFound(result.Error);
        }
    }
}
```

#### ApiKeysController.cs

```csharp
// File: API/Controllers/Admin/ApiKeysController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnormTech.B2B.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/integration/keys")]
    [Authorize(Policy = "integration.manage")]
    public class ApiKeysController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ILogger<ApiKeysController> _logger;
        
        public ApiKeysController(
            IApiKeyService apiKeyService,
            ILogger<ApiKeysController> logger)
        {
            _apiKeyService = apiKeyService;
            _logger = logger;
        }
        
        /// <summary>
        /// Get API keys for a client
        /// </summary>
        [HttpGet("by-client/{clientId}")]
        [ProducesResponseType(typeof(List<ApiKeyListDto>), 200)]
        public async Task<IActionResult> GetKeysByClient(int clientId)
        {
            var result = await _apiKeyService.GetKeysByClientAsync(clientId);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Get API key details
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiKeyDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetKey(int id)
        {
            var result = await _apiKeyService.GetKeyAsync(id);
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.Error);
        }
        
        /// <summary>
        /// Create new API key
        /// </summary>
        /// <remarks>
        /// The plain text key is only returned once in the response.
        /// Store it securely as it cannot be retrieved again.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(CreateApiKeyResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateKey([FromBody] CreateApiKeyDto dto)
        {
            var createdBy = User.Identity?.Name ?? "System";
            var result = await _apiKeyService.CreateKeyAsync(dto, createdBy);
            
            return result.IsSuccess 
                ? CreatedAtAction(nameof(GetKey), new { id = result.Data.Id }, result.Data)
                : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Update API key (name, rate limit, expiration)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiKeyDetailDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateKey(int id, [FromBody] UpdateApiKeyDto dto)
        {
            var updatedBy = User.Identity?.Name ?? "System";
            var result = await _apiKeyService.UpdateKeyAsync(id, dto, updatedBy);
            
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Revoke API key
        /// </summary>
        [HttpPost("{id}/revoke")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RevokeKey(int id, [FromBody] RevokeApiKeyDto dto)
        {
            var revokedBy = User.Identity?.Name ?? "System";
            var result = await _apiKeyService.RevokeKeyAsync(id, dto, revokedBy);
            
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Rotate API key (creates new key, revokes old)
        /// </summary>
        /// <remarks>
        /// Creates a new key with the same permissions and settings,
        /// then revokes the old key. The new plain text key is returned.
        /// </remarks>
        [HttpPost("{id}/rotate")]
        [ProducesResponseType(typeof(CreateApiKeyResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RotateKey(int id)
        {
            var createdBy = User.Identity?.Name ?? "System";
            var result = await _apiKeyService.RotateKeyAsync(id, createdBy);
            
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Update API key permissions
        /// </summary>
        [HttpPut("{id}/permissions")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdatePermissions(
            int id, [FromBody] UpdateApiKeyPermissionsDto dto)
        {
            var result = await _apiKeyService.UpdatePermissionsAsync(id, dto);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Add IP to whitelist
        /// </summary>
        [HttpPost("{id}/ip-whitelist")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddIpToWhitelist(int id, [FromBody] AddIpWhitelistDto dto)
        {
            var result = await _apiKeyService.AddIpToWhitelistAsync(id, dto);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Remove IP from whitelist
        /// </summary>
        [HttpDelete("{id}/ip-whitelist/{whitelistId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveIpFromWhitelist(int id, int whitelistId)
        {
            var result = await _apiKeyService.RemoveIpFromWhitelistAsync(id, whitelistId);
            return result.IsSuccess ? NoContent() : NotFound(result.Error);
        }
        
        /// <summary>
        /// Get API key usage logs
        /// </summary>
        [HttpGet("{id}/usage")]
        [ProducesResponseType(typeof(PagedResult<ApiKeyUsageLogDto>), 200)]
        public async Task<IActionResult> GetUsageLogs(
            int id,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var filter = new UsageLogFilterDto
            {
                ApiKeyId = id,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };
            
            var result = await _apiKeyService.GetUsageLogsAsync(filter);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Get API key usage statistics
        /// </summary>
        [HttpGet("{id}/stats")]
        [ProducesResponseType(typeof(ApiKeyUsageStatsDto), 200)]
        public async Task<IActionResult> GetUsageStats(
            int id,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var to = toDate ?? DateTime.UtcNow;
            
            var result = await _apiKeyService.GetUsageStatsAsync(id, from, to);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
        
        /// <summary>
        /// Get available permission scopes
        /// </summary>
        [HttpGet("available-scopes")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public IActionResult GetAvailableScopes()
        {
            return Ok(IntegrationPermissionScopes.GetAllScopes());
        }
    }
}
```

---

## Authentication Handler

### ApiKeyAuthenticationHandler.cs

```csharp
// File: API/Authentication/ApiKeyAuthenticationHandler.cs
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace EnormTech.B2B.API.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public string HeaderName { get; set; } = "X-API-Key";
    }
    
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IApiKeyService _apiKeyService;
        
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IApiKeyService apiKeyService)
            : base(options, logger, encoder)
        {
            _apiKeyService = apiKeyService;
        }
        
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Get API key from header
            if (!Request.Headers.TryGetValue(Options.HeaderName, out var apiKeyHeaderValues))
            {
                return AuthenticateResult.NoResult();
            }
            
            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            
            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return AuthenticateResult.NoResult();
            }
            
            // Get client IP
            var ipAddress = GetClientIpAddress();
            
            // Validate API key
            var validationResult = await _apiKeyService.ValidateKeyAsync(providedApiKey, ipAddress);
            
            if (!validationResult.IsValid)
            {
                Logger.LogWarning(
                    "API key validation failed: {ErrorCode} - {ErrorMessage} from IP {IpAddress}",
                    validationResult.ErrorCode,
                    validationResult.ErrorMessage,
                    ipAddress);
                
                return AuthenticateResult.Fail(validationResult.ErrorMessage ?? "Invalid API key");
            }
            
            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, validationResult.ApiKeyId.ToString()!),
                new Claim("ApiClientId", validationResult.ApiClientId.ToString()!),
                new Claim("ApiClientName", validationResult.ClientName ?? ""),
                new Claim("AuthenticationType", "ApiKey"),
                new Claim("RateLimitPerMinute", validationResult.RateLimitPerMinute.ToString())
            };
            
            // Add permission claims
            foreach (var permission in validationResult.Permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }
            
            var identity = new ClaimsIdentity(claims, Options.DefaultScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Options.DefaultScheme);
            
            // Store validation result in HttpContext for later use
            Context.Items["ApiKeyValidationResult"] = validationResult;
            
            return AuthenticateResult.Success(ticket);
        }
        
        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.Headers.Append("WWW-Authenticate", $"ApiKey realm=\"Integration API\"");
            return Task.CompletedTask;
        }
        
        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            return Task.CompletedTask;
        }
        
        private string GetClientIpAddress()
        {
            // Check for forwarded headers (when behind proxy/load balancer)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }
            
            return Context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
```

### IntegrationPermissionAttribute.cs

```csharp
// File: API/Authorization/IntegrationPermissionAttribute.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EnormTech.B2B.API.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireIntegrationPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _requiredScope;
        
        public RequireIntegrationPermissionAttribute(string scope)
        {
            _requiredScope = scope;
        }
        
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            
            var permissions = user.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();
            
            // Check for exact match, wildcard, or resource wildcard
            var hasPermission = permissions.Any(p => 
                p == _requiredScope ||
                p == "*" ||
                (_requiredScope.Contains(':') && p == _requiredScope.Split(':')[0] + ":*"));
            
            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
```

---

## Middleware Components

### RateLimitingMiddleware.cs

```csharp
// File: API/Middleware/RateLimitingMiddleware.cs
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace EnormTech.B2B.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        
        public RateLimitingMiddleware(
            RequestDelegate next,
            IDistributedCache cache,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            // Only apply to authenticated API key requests
            var apiKeyId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rateLimitClaim = context.User.FindFirst("RateLimitPerMinute")?.Value;
            
            if (string.IsNullOrEmpty(apiKeyId) || string.IsNullOrEmpty(rateLimitClaim))
            {
                await _next(context);
                return;
            }
            
            var rateLimit = int.Parse(rateLimitClaim);
            var cacheKey = $"ratelimit:{apiKeyId}:{DateTime.UtcNow:yyyyMMddHHmm}";
            
            var currentCountStr = await _cache.GetStringAsync(cacheKey);
            var currentCount = string.IsNullOrEmpty(currentCountStr) ? 0 : int.Parse(currentCountStr);
            
            if (currentCount >= rateLimit)
            {
                _logger.LogWarning("Rate limit exceeded for API key {ApiKeyId}", apiKeyId);
                
                context.Response.StatusCode = 429;
                context.Response.Headers.Append("Retry-After", "60");
                context.Response.Headers.Append("X-RateLimit-Limit", rateLimit.ToString());
                context.Response.Headers.Append("X-RateLimit-Remaining", "0");
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "rate_limit_exceeded",
                    message = "Too many requests. Please try again later.",
                    retryAfter = 60
                });
                
                return;
            }
            
            // Increment counter
            currentCount++;
            await _cache.SetStringAsync(cacheKey, currentCount.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });
            
            // Add rate limit headers
            context.Response.Headers.Append("X-RateLimit-Limit", rateLimit.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", (rateLimit - currentCount).ToString());
            
            await _next(context);
        }
    }
}
```

### ApiKeyUsageLoggingMiddleware.cs

```csharp
// File: API/Middleware/ApiKeyUsageLoggingMiddleware.cs
using System.Diagnostics;

namespace EnormTech.B2B.API.Middleware
{
    public class ApiKeyUsageLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyUsageLoggingMiddleware> _logger;
        
        public ApiKeyUsageLoggingMiddleware(
            RequestDelegate next,
            ILogger<ApiKeyUsageLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context, IApiKeyUsageLogRepository logRepository)
        {
            var apiKeyIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(apiKeyIdClaim) || !int.TryParse(apiKeyIdClaim, out var apiKeyId))
            {
                await _next(context);
                return;
            }
            
            var stopwatch = Stopwatch.StartNew();
            string? errorMessage = null;
            
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                throw;
            }
            finally
            {
                stopwatch.Stop();
                
                try
                {
                    var log = ApiKeyUsageLog.Create(
                        apiKeyId,
                        context.Request.Path.Value ?? "/",
                        context.Request.Method,
                        GetClientIpAddress(context),
                        context.Request.Headers["User-Agent"].FirstOrDefault(),
                        context.Response.StatusCode,
                        (int)stopwatch.ElapsedMilliseconds,
                        errorMessage);
                    
                    // Fire and forget - don't block the response
                    _ = logRepository.AddAsync(log);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to log API key usage");
                }
            }
        }
        
        private static string GetClientIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').First().Trim();
            }
            
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
```

---

## Configuration

### Program.cs Registration

```csharp
// Integration API Authentication Configuration
builder.Services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.DefaultScheme, 
        options => { options.HeaderName = "X-API-Key"; });

// Services
builder.Services.AddScoped<IApiKeyGenerator, ApiKeyGenerator>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IApiClientRepository, ApiClientRepository>();
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
builder.Services.AddScoped<IApiKeyUsageLogRepository, ApiKeyUsageLogRepository>();

// Redis cache for rate limiting
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "IntegrationApi:";
});

// Middleware pipeline (in order)
app.UseAuthentication();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<ApiKeyUsageLoggingMiddleware>();
app.UseAuthorization();
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EnormTech_B2B;Username=postgres;Password=***",
    "Redis": "localhost:6379"
  },
  "IntegrationApi": {
    "DefaultRateLimitPerMinute": 500,
    "MaxRateLimitPerMinute": 10000,
    "UsageLogRetentionDays": 90,
    "KeyExpirationWarningDays": 30
  }
}
```

---

## Security Considerations

### Key Security

1. **Hashing**: All API keys stored as SHA-256 hashes
2. **One-time display**: Plain text key only shown once at creation
3. **No key retrieval**: Cannot retrieve plain text key from database
4. **Secure generation**: Cryptographically secure random generation

### Access Control

1. **Permission scopes**: Granular control over API access
2. **IP whitelisting**: Optional additional security layer
3. **Rate limiting**: Prevent abuse and DDoS
4. **Expiration**: Keys can have mandatory expiration

### Audit Trail

1. **Usage logging**: All requests logged with details
2. **Key lifecycle**: Creation, rotation, revocation tracked
3. **Admin actions**: All management operations logged

### Recommendations

1. **Key rotation**: Rotate keys every 90 days
2. **Minimum permissions**: Grant only required scopes
3. **IP restriction**: Use IP whitelist for production keys
4. **Monitoring**: Alert on unusual usage patterns
5. **Expiration**: Set expiration for all production keys

---

## Summary

This specification provides:

✅ **Complete database design** for API clients, keys, permissions, IP whitelist, and usage logs  
✅ **Domain entities** with factory methods and business logic  
✅ **Application layer** with DTOs, validators, and service interfaces  
✅ **Infrastructure layer** with repositories and service implementations  
✅ **Admin API endpoints** for complete key lifecycle management  
✅ **Authentication handler** for API key validation  
✅ **Middleware** for rate limiting and usage logging  
✅ **Security best practices** including hashing, scopes, and audit trails  

---

**Document Version**: 1.0  
**Created**: December 2025  
**For**: B2B E-Commerce Platform - Integration API  
**Framework**: .NET 8, EF Core 8, Clean Architecture
