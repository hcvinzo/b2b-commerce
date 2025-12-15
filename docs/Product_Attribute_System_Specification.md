# Product Attribute System Specification

**Vesmarket B2B E-Commerce Platform**

| Field | Value |
|-------|-------|
| Version | 1.0 |
| Date | December 2025 |
| Project | Vesmarket B2B E-Commerce Platform |
| Target Stack | .NET 8, EF Core 8, PostgreSQL, Clean Architecture |

---

## 1. Executive Summary

This document specifies the Product Attribute System architecture for the Vesmarket B2B e-commerce platform. The system enables flexible product attribute management while supporting multi-category product placement, a critical requirement for B2B catalogs where products like SD cards may logically belong to multiple categories (Computer Storage, Camera Accessories, Phone Accessories).

The recommended approach separates attribute definition (Product Type) from navigation concerns (Category), following industry-standard patterns used by major e-commerce platforms including Magento, Shopify Plus, and SAP Commerce Cloud.

---

## 2. Problem Statement

### 2.1 The Multi-Category Product Challenge

Category-based attribute management breaks down when products logically belong to multiple categories. Consider an SD memory card:

1. Under "Computer Storage Accessories": Requires Interface Type, Read Speed, Write Speed, Capacity
2. Under "Camera Accessories": Requires Camera Compatibility, Video Recording Support, Speed Class
3. Question: Which attributes apply? Union creates duplicates; intersection loses specificity

### 2.2 Category-Based Approach Limitations

| Issue | Impact |
|-------|--------|
| Attribute Conflicts | Same product in multiple categories may have conflicting attribute requirements |
| Data Duplication | Common attributes (Capacity, Brand) must be defined in each category |
| Validation Complexity | Which category's rules apply when saving a multi-category product? |
| ERP Sync Issues | LOGO ERP expects consistent attribute definitions per product type |

---

## 3. Solution Architecture

### 3.1 Core Principle: Separation of Concerns

The solution separates two distinct concerns that are often incorrectly merged:

| Concept | Purpose | Relationship to Product |
|---------|---------|------------------------|
| **Product Type** | Defines what attributes a product has (template) | ONE per product (strict) |
| **Category** | Navigation, browsing, SEO, filtering UI | MANY per product (flexible) |

### 3.2 Entity Relationship Overview

```
Product (1) ──── (1) ProductType ──── (*) ProductTypeAttribute ──── (1) AttributeDefinition
    │                                                                      │
    │                                                                      │
    └──── (*) ProductCategory ──── (1) Category          AttributeValue (predefined options)
    └──── (*) ProductAttributeValue (actual values)
```

### 3.3 Key Design Decisions

- **Product Type defines attributes**: A product's available attributes are determined solely by its ProductType, not its categories
- **Categories are for navigation only**: Products can belong to multiple categories without attribute conflicts
- **Primary category for SEO**: One category is marked as primary for canonical URLs and breadcrumbs
- **Typed value storage**: ProductAttributeValue uses separate columns for different data types (text, numeric, boolean, etc.) enabling efficient filtering and validation

---

## 4. Domain Entity Specifications

### 4.1 AttributeDefinition

Master definition of an attribute that can be reused across multiple product types.

```csharp
public class AttributeDefinition : BaseEntity
{
    public string Code { get; private set; }           // "screen_size", "capacity"
    public string Name { get; private set; }           // "Ekran Boyutu", "Kapasite"
    public string? NameEn { get; private set; }        // "Screen Size", "Capacity"
    public AttributeType Type { get; private set; }    // Text, Number, Select, etc.
    public string? Unit { get; private set; }          // "inch", "GB", "MB/s"
    public bool IsFilterable { get; private set; }     // Show in product filters?
    public bool IsRequired { get; private set; }       // Default required status
    public bool IsVisibleOnProductPage { get; private set; }
    public int DisplayOrder { get; private set; }
    
    // Navigation
    public ICollection<AttributeValue> PredefinedValues { get; private set; }
    public ICollection<ProductTypeAttribute> ProductTypeAttributes { get; private set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Primary key |
| `Code` | string | Unique identifier (e.g., "screen_size", "capacity") |
| `Name` | string | Display name in Turkish (e.g., "Ekran Boyutu") |
| `NameEn` | string? | Display name in English (e.g., "Screen Size") |
| `Type` | AttributeType | Text, Number, Select, MultiSelect, Boolean, Date |
| `Unit` | string? | Unit of measurement (e.g., "inch", "GB", "MB/s") |
| `IsFilterable` | bool | Show in product listing filters |
| `IsRequired` | bool | Default required status (can be overridden) |
| `IsVisibleOnProductPage` | bool | Display on product detail page |
| `DisplayOrder` | int | Ordering in UI |

### 4.2 AttributeType Enumeration

```csharp
public enum AttributeType
{
    Text = 1,        // Free text input
    Number = 2,      // Numeric value (with optional unit)
    Select = 3,      // Single selection from predefined values
    MultiSelect = 4, // Multiple selection from predefined values
    Boolean = 5,     // Yes/No
    Date = 6         // Date value
}
```

| Value | Name | Usage |
|-------|------|-------|
| 1 | Text | Free text input (e.g., Model Number) |
| 2 | Number | Numeric value with optional unit (e.g., Weight: 2.5 kg) |
| 3 | Select | Single selection from predefined values (e.g., Color) |
| 4 | MultiSelect | Multiple selections (e.g., Compatible Devices) |
| 5 | Boolean | Yes/No toggle (e.g., Wireless) |
| 6 | Date | Date value (e.g., Release Date) |

### 4.3 AttributeValue

Predefined values for Select and MultiSelect attribute types.

```csharp
public class AttributeValue : BaseEntity
{
    public int AttributeDefinitionId { get; private set; }
    public string Value { get; private set; }          // "256", "512", "1024"
    public string? DisplayText { get; private set; }   // "256 GB", "512 GB", "1 TB"
    public int DisplayOrder { get; private set; }
    
    // Navigation
    public AttributeDefinition AttributeDefinition { get; private set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Primary key |
| `AttributeDefinitionId` | int | FK to parent AttributeDefinition |
| `Value` | string | Internal value (e.g., "256", "512", "1024") |
| `DisplayText` | string? | User-facing text (e.g., "256 GB", "512 GB", "1 TB") |
| `DisplayOrder` | int | Ordering in dropdowns |

### 4.4 ProductType

Attribute template that defines which attributes are available for products of this type.

```csharp
public class ProductType : BaseEntity
{
    public string Code { get; private set; }        // "memory_card", "laptop"
    public string Name { get; private set; }        // "Hafıza Kartı"
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    
    // Navigation
    public ICollection<ProductTypeAttribute> Attributes { get; private set; }
    public ICollection<Product> Products { get; private set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Primary key |
| `Code` | string | Unique code (e.g., "memory_card", "laptop", "printer") |
| `Name` | string | Display name (e.g., "Hafıza Kartı") |
| `Description` | string? | Optional description for admin use |
| `IsActive` | bool | Whether new products can use this type |

### 4.5 ProductTypeAttribute

Junction entity binding attributes to product types with type-specific configuration.

```csharp
public class ProductTypeAttribute : BaseEntity
{
    public int ProductTypeId { get; private set; }
    public int AttributeDefinitionId { get; private set; }
    public bool IsRequired { get; private set; }       // Override at type level
    public int DisplayOrder { get; private set; }
    
    // Navigation
    public ProductType ProductType { get; private set; }
    public AttributeDefinition AttributeDefinition { get; private set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `ProductTypeId` | int | FK to ProductType |
| `AttributeDefinitionId` | int | FK to AttributeDefinition |
| `IsRequired` | bool | Override: required for this product type |
| `DisplayOrder` | int | Ordering within this product type |

### 4.6 Category (Navigation Only)

Categories are used purely for navigation and browsing. They do NOT define attributes.

```csharp
public class Category : BaseEntity
{
    public string Name { get; private set; }
    public string? NameEn { get; private set; }
    public string Slug { get; private set; }
    public int? ParentCategoryId { get; private set; }
    public int? DefaultProductTypeId { get; private set; }  // Suggested type
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    
    // Navigation
    public Category? ParentCategory { get; private set; }
    public ICollection<Category> SubCategories { get; private set; }
    public ICollection<ProductCategory> ProductCategories { get; private set; }
    public ProductType? DefaultProductType { get; private set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Primary key |
| `Name` | string | Category name in Turkish |
| `NameEn` | string? | Category name in English |
| `Slug` | string | URL-friendly identifier |
| `ParentCategoryId` | int? | FK to parent for hierarchy |
| `DefaultProductTypeId` | int? | Suggested ProductType when adding products (optional) |
| `ImageUrl` | string? | Category image for UI |
| `DisplayOrder` | int | Ordering in navigation |
| `IsActive` | bool | Visibility in storefront |

### 4.7 ProductCategory

Many-to-many relationship allowing products to appear in multiple categories.

```csharp
public class ProductCategory : BaseEntity
{
    public int ProductId { get; private set; }
    public int CategoryId { get; private set; }
    public bool IsPrimary { get; private set; }  // For canonical URL
    public int DisplayOrder { get; private set; }
    
    // Navigation
    public Product Product { get; private set; }
    public Category Category { get; private set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `ProductId` | int | FK to Product |
| `CategoryId` | int | FK to Category |
| `IsPrimary` | bool | Primary category for canonical URL and breadcrumbs |
| `DisplayOrder` | int | Product ordering within this category |

### 4.8 Product (Updated)

Product entity now references ProductType instead of a single Category.

```csharp
public class Product : BaseEntity, IAggregateRoot
{
    // Identity
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    
    // Type (defines available attributes)
    public int ProductTypeId { get; private set; }
    
    // Brand
    public int BrandId { get; private set; }
    
    // Pricing
    public Money ListPrice { get; private set; }
    public Money DealerPrice { get; private set; }
    public string Currency { get; private set; }
    
    // Stock
    public int StockQuantity { get; private set; }
    public bool TrackStock { get; private set; }
    
    // Status
    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }
    
    // Navigation
    public ProductType ProductType { get; private set; }
    public Brand Brand { get; private set; }
    public ICollection<ProductCategory> ProductCategories { get; private set; }
    public ICollection<ProductAttributeValue> AttributeValues { get; private set; }
    public ICollection<ProductImage> Images { get; private set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `Id` | int | Primary key |
| `Sku` | string | Stock Keeping Unit (unique) |
| `Name` | string | Product name |
| `ProductTypeId` | int | FK to ProductType (defines available attributes) |
| `BrandId` | int | FK to Brand |
| `ProductCategories` | Collection | Navigation: multiple category assignments |
| `AttributeValues` | Collection | Actual attribute values for this product |

### 4.9 ProductAttributeValue

Stores the actual attribute values for each product with type-specific columns.

```csharp
public class ProductAttributeValue : BaseEntity
{
    public int ProductId { get; private set; }
    public int AttributeDefinitionId { get; private set; }
    
    // Flexible value storage (only one is populated based on attribute type)
    public string? TextValue { get; private set; }
    public decimal? NumericValue { get; private set; }
    public int? AttributeValueId { get; private set; }  // FK for Select type
    public bool? BooleanValue { get; private set; }
    public DateTime? DateValue { get; private set; }
    public string? MultiSelectValueIds { get; private set; }  // JSON: "[1,3,5]"
    
    // Navigation
    public Product Product { get; private set; }
    public AttributeDefinition AttributeDefinition { get; private set; }
    public AttributeValue? SelectedValue { get; private set; }
}
```

| Property | Type | Description |
|----------|------|-------------|
| `ProductId` | int | FK to Product |
| `AttributeDefinitionId` | int | FK to AttributeDefinition |
| `TextValue` | string? | Value for Text type attributes |
| `NumericValue` | decimal? | Value for Number type attributes |
| `AttributeValueId` | int? | FK to AttributeValue for Select type |
| `BooleanValue` | bool? | Value for Boolean type attributes |
| `DateValue` | DateTime? | Value for Date type attributes |
| `MultiSelectValueIds` | string? | JSON array of AttributeValue IDs for MultiSelect |

---

## 5. Example: SD Card Product

This example demonstrates how the architecture handles the multi-category SD card scenario.

### 5.1 ProductType Definition

```
ProductType: "Memory Card" (Code: memory_card)
├── Capacity (Select: 32GB, 64GB, 128GB, 256GB, 512GB, 1TB) [Required]
├── Speed Class (Select: Class 10, UHS-I, UHS-II, UHS-III)
├── Read Speed (Number, Unit: MB/s)
├── Write Speed (Number, Unit: MB/s)
├── Form Factor (Select: SD, microSD, CFexpress)
└── Interface (Select: UHS-I, UHS-II, PCIe)
```

### 5.2 Product Instance

```
Product: "SanDisk Extreme Pro 128GB microSD"
├── ProductType: Memory Card
├── Categories:
│   ├── Computer Storage [PRIMARY - used for canonical URL]
│   ├── Camera Accessories
│   └── Phone Accessories
└── AttributeValues:
    ├── Capacity: 128GB
    ├── Speed Class: UHS-III
    ├── Read Speed: 200 MB/s
    ├── Write Speed: 140 MB/s
    └── Form Factor: microSD
```

### 5.3 How It Works

1. **Admin creates ProductType "Memory Card"** with relevant attributes
2. **Admin creates categories** for navigation (Computer Storage, Camera Accessories, etc.)
3. **When adding product**: Admin selects ProductType first, system shows only relevant attributes
4. **Admin assigns categories**: Product can be in multiple categories for discoverability
5. **On category page**: System dynamically builds filters from attributes of products in that category

---

## 6. Database Schema (PostgreSQL)

### 6.1 AttributeDefinitions

```sql
CREATE TABLE AttributeDefinitions (
    Id SERIAL PRIMARY KEY,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Name VARCHAR(100) NOT NULL,
    NameEn VARCHAR(100),
    Type SMALLINT NOT NULL,  -- 1=Text, 2=Number, 3=Select, etc.
    Unit VARCHAR(20),
    IsFilterable BOOLEAN DEFAULT false,
    IsRequired BOOLEAN DEFAULT false,
    IsVisibleOnProductPage BOOLEAN DEFAULT true,
    DisplayOrder INT DEFAULT 0,
    IsDeleted BOOLEAN DEFAULT false,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ
);

CREATE INDEX IX_AttributeDefinitions_Code ON AttributeDefinitions(Code);
CREATE INDEX IX_AttributeDefinitions_IsFilterable ON AttributeDefinitions(IsFilterable) WHERE IsFilterable = true;
```

### 6.2 AttributeValues

```sql
CREATE TABLE AttributeValues (
    Id SERIAL PRIMARY KEY,
    AttributeDefinitionId INT NOT NULL REFERENCES AttributeDefinitions(Id) ON DELETE CASCADE,
    Value VARCHAR(200) NOT NULL,
    DisplayText VARCHAR(200),
    DisplayOrder INT DEFAULT 0,
    IsDeleted BOOLEAN DEFAULT false,
    CreatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IX_AttributeValues_Definition ON AttributeValues(AttributeDefinitionId);
```

### 6.3 ProductTypes

```sql
CREATE TABLE ProductTypes (
    Id SERIAL PRIMARY KEY,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    IsActive BOOLEAN DEFAULT true,
    IsDeleted BOOLEAN DEFAULT false,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ
);

CREATE INDEX IX_ProductTypes_Code ON ProductTypes(Code);
CREATE INDEX IX_ProductTypes_IsActive ON ProductTypes(IsActive) WHERE IsActive = true;
```

### 6.4 ProductTypeAttributes

```sql
CREATE TABLE ProductTypeAttributes (
    Id SERIAL PRIMARY KEY,
    ProductTypeId INT NOT NULL REFERENCES ProductTypes(Id) ON DELETE CASCADE,
    AttributeDefinitionId INT NOT NULL REFERENCES AttributeDefinitions(Id) ON DELETE CASCADE,
    IsRequired BOOLEAN DEFAULT false,
    DisplayOrder INT DEFAULT 0,
    UNIQUE(ProductTypeId, AttributeDefinitionId)
);

CREATE INDEX IX_ProductTypeAttributes_ProductType ON ProductTypeAttributes(ProductTypeId);
CREATE INDEX IX_ProductTypeAttributes_Attribute ON ProductTypeAttributes(AttributeDefinitionId);
```

### 6.5 Categories

```sql
CREATE TABLE Categories (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    NameEn VARCHAR(100),
    Slug VARCHAR(100) NOT NULL UNIQUE,
    ParentCategoryId INT REFERENCES Categories(Id) ON DELETE SET NULL,
    DefaultProductTypeId INT REFERENCES ProductTypes(Id) ON DELETE SET NULL,
    ImageUrl VARCHAR(500),
    DisplayOrder INT DEFAULT 0,
    IsActive BOOLEAN DEFAULT true,
    IsDeleted BOOLEAN DEFAULT false,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ
);

CREATE INDEX IX_Categories_Parent ON Categories(ParentCategoryId);
CREATE INDEX IX_Categories_Slug ON Categories(Slug);
CREATE INDEX IX_Categories_IsActive ON Categories(IsActive) WHERE IsActive = true;
```

### 6.6 Products (Updated)

```sql
CREATE TABLE Products (
    Id SERIAL PRIMARY KEY,
    Sku VARCHAR(50) NOT NULL UNIQUE,
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    ProductTypeId INT NOT NULL REFERENCES ProductTypes(Id) ON DELETE RESTRICT,
    BrandId INT NOT NULL REFERENCES Brands(Id) ON DELETE RESTRICT,
    ListPrice DECIMAL(18,2) NOT NULL,
    DealerPrice DECIMAL(18,2) NOT NULL,
    Currency VARCHAR(3) DEFAULT 'TRY',
    StockQuantity INT DEFAULT 0,
    TrackStock BOOLEAN DEFAULT true,
    IsActive BOOLEAN DEFAULT true,
    IsFeatured BOOLEAN DEFAULT false,
    IsDeleted BOOLEAN DEFAULT false,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ
);

CREATE INDEX IX_Products_Sku ON Products(Sku);
CREATE INDEX IX_Products_ProductType ON Products(ProductTypeId);
CREATE INDEX IX_Products_Brand ON Products(BrandId);
CREATE INDEX IX_Products_IsActive ON Products(IsActive) WHERE IsActive = true;
```

### 6.7 ProductCategories

```sql
CREATE TABLE ProductCategories (
    Id SERIAL PRIMARY KEY,
    ProductId INT NOT NULL REFERENCES Products(Id) ON DELETE CASCADE,
    CategoryId INT NOT NULL REFERENCES Categories(Id) ON DELETE CASCADE,
    IsPrimary BOOLEAN DEFAULT false,
    DisplayOrder INT DEFAULT 0,
    UNIQUE(ProductId, CategoryId)
);

CREATE INDEX IX_ProductCategories_Product ON ProductCategories(ProductId);
CREATE INDEX IX_ProductCategories_Category ON ProductCategories(CategoryId);

-- Ensure only one primary category per product
CREATE UNIQUE INDEX IX_ProductCategories_Primary 
ON ProductCategories(ProductId) WHERE IsPrimary = true;
```

### 6.8 ProductAttributeValues

```sql
CREATE TABLE ProductAttributeValues (
    Id SERIAL PRIMARY KEY,
    ProductId INT NOT NULL REFERENCES Products(Id) ON DELETE CASCADE,
    AttributeDefinitionId INT NOT NULL REFERENCES AttributeDefinitions(Id) ON DELETE CASCADE,
    TextValue TEXT,
    NumericValue DECIMAL(18,4),
    AttributeValueId INT REFERENCES AttributeValues(Id) ON DELETE SET NULL,
    BooleanValue BOOLEAN,
    DateValue DATE,
    MultiSelectValueIds JSONB,  -- For multi-select: [1, 3, 5]
    UNIQUE(ProductId, AttributeDefinitionId)
);

CREATE INDEX IX_ProductAttributeValues_Product ON ProductAttributeValues(ProductId);
CREATE INDEX IX_ProductAttributeValues_Definition ON ProductAttributeValues(AttributeDefinitionId);
CREATE INDEX IX_ProductAttributeValues_NumericValue ON ProductAttributeValues(AttributeDefinitionId, NumericValue);
CREATE INDEX IX_ProductAttributeValues_AttributeValue ON ProductAttributeValues(AttributeValueId);
```

---

## 7. ERP Integration (LOGO)

The Integration API provides endpoints for LOGO ERP to sync product types, attributes, and products.

### 7.1 GET /api/integration/product-types

Returns all product types with their attribute definitions.

**Response:**
```json
{
  "productTypes": [
    {
      "code": "memory_card",
      "name": "Hafıza Kartı",
      "attributes": [
        { 
          "code": "capacity", 
          "name": "Kapasite",
          "type": "Select", 
          "required": true,
          "values": ["32GB", "64GB", "128GB", "256GB", "512GB", "1TB"]
        },
        { 
          "code": "read_speed", 
          "name": "Okuma Hızı",
          "type": "Number", 
          "unit": "MB/s",
          "required": false
        }
      ]
    }
  ]
}
```

### 7.2 GET /api/integration/categories

Returns category tree for navigation setup.

**Response:**
```json
{
  "categories": [
    { 
      "id": 1, 
      "name": "Bilgisayar Depolama", 
      "slug": "bilgisayar-depolama",
      "parentId": null,
      "defaultProductTypeCode": "storage_device"
    },
    { 
      "id": 2, 
      "name": "Kamera Aksesuarları", 
      "slug": "kamera-aksesuarlari",
      "parentId": null,
      "defaultProductTypeCode": null
    }
  ]
}
```

### 7.3 POST /api/integration/products

Create or update products with type, categories, and attributes.

**Request:**
```json
{
  "sku": "SDXC-128-EP",
  "name": "SanDisk Extreme Pro 128GB",
  "description": "Profesyonel kullanım için yüksek hızlı microSD kart",
  "productTypeCode": "memory_card",
  "brandCode": "sandisk",
  "listPrice": 1299.99,
  "dealerPrice": 999.99,
  "currency": "TRY",
  "stockQuantity": 150,
  "categoryIds": [1, 2, 5],
  "primaryCategoryId": 1,
  "attributes": {
    "capacity": "128GB",
    "read_speed": 200,
    "write_speed": 140,
    "speed_class": "UHS-III",
    "form_factor": "microSD"
  },
  "images": [
    { "url": "https://cdn.example.com/sdcard-front.jpg", "isPrimary": true },
    { "url": "https://cdn.example.com/sdcard-back.jpg", "isPrimary": false }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "productId": 12345,
  "sku": "SDXC-128-EP",
  "message": "Product created successfully"
}
```

### 7.4 GET /api/integration/products

Returns products for ERP sync with full attribute data.

**Query Parameters:**
- `updatedSince`: ISO datetime for incremental sync
- `productTypeCode`: Filter by product type
- `page`, `pageSize`: Pagination

**Response:**
```json
{
  "products": [
    {
      "sku": "SDXC-128-EP",
      "name": "SanDisk Extreme Pro 128GB",
      "productTypeCode": "memory_card",
      "brandCode": "sandisk",
      "listPrice": 1299.99,
      "dealerPrice": 999.99,
      "stockQuantity": 150,
      "categories": ["bilgisayar-depolama", "kamera-aksesuarlari"],
      "primaryCategory": "bilgisayar-depolama",
      "attributes": {
        "capacity": "128GB",
        "read_speed": 200,
        "write_speed": 140
      },
      "updatedAt": "2025-12-15T10:30:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalCount": 1250,
    "totalPages": 25
  }
}
```

---

## 8. Dynamic Filtering Strategy

When a user browses a category, the system dynamically builds filter options based on products actually present in that category.

### 8.1 Filter Aggregation Service

```csharp
public class CategoryFilterService
{
    private readonly ApplicationDbContext _context;
    
    public async Task<CategoryFilters> GetFiltersForCategoryAsync(int categoryId)
    {
        // Get all product IDs in this category (including subcategories if needed)
        var productIds = await _context.ProductCategories
            .Where(pc => pc.CategoryId == categoryId)
            .Select(pc => pc.ProductId)
            .ToListAsync();
        
        // Get distinct filterable attributes used by these products
        var filters = await _context.ProductAttributeValues
            .Where(pav => productIds.Contains(pav.ProductId))
            .Include(pav => pav.AttributeDefinition)
            .Include(pav => pav.SelectedValue)
            .Where(pav => pav.AttributeDefinition.IsFilterable)
            .GroupBy(pav => new { 
                pav.AttributeDefinitionId, 
                pav.AttributeDefinition.Name,
                pav.AttributeDefinition.Type,
                pav.AttributeDefinition.Unit
            })
            .Select(g => new FilterOption
            {
                AttributeId = g.Key.AttributeDefinitionId,
                AttributeName = g.Key.Name,
                AttributeType = g.Key.Type,
                Unit = g.Key.Unit,
                AvailableValues = g.Key.Type == AttributeType.Select
                    ? g.Where(x => x.SelectedValue != null)
                         .Select(x => x.SelectedValue!.DisplayText ?? x.SelectedValue.Value)
                         .Distinct()
                         .ToList()
                    : g.Key.Type == AttributeType.Number
                        ? new List<string> { 
                            g.Min(x => x.NumericValue).ToString(), 
                            g.Max(x => x.NumericValue).ToString() 
                          }
                        : new List<string>()
            })
            .ToListAsync();
        
        return new CategoryFilters { Filters = filters };
    }
}
```

### 8.2 Filter Response Example

```json
{
  "categoryId": 1,
  "categoryName": "Computer Storage",
  "filters": [
    {
      "attributeId": 5,
      "attributeName": "Capacity",
      "attributeType": "Select",
      "values": ["64GB", "128GB", "256GB", "512GB"]
    },
    {
      "attributeId": 8,
      "attributeName": "Read Speed",
      "attributeType": "Number",
      "unit": "MB/s",
      "range": { "min": 95, "max": 300 }
    },
    {
      "attributeId": 12,
      "attributeName": "Form Factor",
      "attributeType": "Select",
      "values": ["SD", "microSD", "CFexpress"]
    }
  ]
}
```

---

## 9. Benefits Summary

| Aspect | Benefit |
|--------|---------|
| **Multi-Category** | Products can appear in unlimited categories without attribute conflicts |
| **Attribute Consistency** | Each product type guarantees consistent attributes across all products |
| **Filtering** | Dynamic filters per category based on products present (not predefined) |
| **ERP Integration** | Clean mapping: ProductType → ERP Product Group, Attributes sync per type |
| **Scalability** | Add new attributes/types without schema changes |
| **Validation** | Type-specific validation with required field enforcement per product type |
| **SEO** | Primary category provides canonical URL; secondary categories add discovery |
| **Admin UX** | ProductType selection guides admin to relevant attributes only |

---

## 10. Migration from Category-Based Attributes

If migrating from an existing category-based attribute system:

### 10.1 Migration Steps

1. **Analyze existing categories**: Identify unique attribute sets per category
2. **Create ProductTypes**: Group categories with identical attributes into ProductTypes
3. **Create AttributeDefinitions**: Extract unique attributes from all categories
4. **Map ProductType → Attributes**: Bind attributes to appropriate product types
5. **Update Products**: Assign ProductTypeId based on original category
6. **Create ProductCategories**: Convert single CategoryId to many-to-many relationships
7. **Migrate attribute values**: Transform existing attribute data to ProductAttributeValues

### 10.2 Backward Compatibility

During migration, maintain backward compatibility:

```csharp
public class Product
{
    // New structure
    public int ProductTypeId { get; private set; }
    public ICollection<ProductCategory> ProductCategories { get; private set; }
    
    // Computed property for backward compatibility
    public int? PrimaryCategoryId => ProductCategories
        ?.FirstOrDefault(pc => pc.IsPrimary)?.CategoryId;
}
```

---

## 11. Document Approval

| Role | Name | Date |
|------|------|------|
| Author | | |
| Technical Review | | |
| Approval | | |

---

*Document Version 1.0 - December 2025*
