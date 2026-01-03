# Order & Quotation Domain Design Specification
## Vesmarket B2B E-Commerce Platform

**Version**: 1.0  
**Created**: January 2025  
**Status**: Design Phase

---

## 1. Executive Summary

This document defines the order and quotation domain design for the Vesmarket B2B e-commerce platform. The design addresses complex B2B requirements including quotation workflows, multi-user dealer accounts, tiered pricing, multi-currency operations, various payment methods, and parametric approval workflows.

---

## 2. Core Requirements Addressed

| Requirement | Solution Approach |
|-------------|-------------------|
| Quotation Feature | Separate Quotation aggregate with conversion to Order |
| Multi-user Dealers | User → Customer relationship with role-based permissions |
| Price Tiers (1-5) | Pricing hierarchy: Special → Campaign → Tier (1-5) → List |
| Multi-currency | Customer preferred currency + exchange rate locking |
| Multiple Payment Methods | PaymentMethod enum + Payment aggregate |
| Order Approval | Parametric rules + rule-based engine |
| Cancellation | OrderCancellation aggregate with approval workflow |
| Return | Return aggregate with inspection workflow |

---

## 3. Domain Model Overview

### 3.1 High-Level Entity Relationship

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              CUSTOMER DOMAIN                                     │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│   ┌──────────────┐         ┌──────────────┐         ┌──────────────┐            │
│   │   Customer   │ 1────n  │     User     │         │   Address    │            │
│   │  (Dealer)    │─────────│  (Employee)  │         │              │            │
│   └──────────────┘         └──────────────┘         └──────────────┘            │
│          │                        │                        │                     │
│          │ has                    │ creates                │ used in             │
│          ▼                        ▼                        ▼                     │
│   ┌──────────────┐         ┌──────────────┐         ┌──────────────┐            │
│   │ Credit Limit │         │  Price Tier  │         │   Quotation  │            │
│   │   Settings   │         │    (1-5)     │         │    Order     │            │
│   └──────────────┘         └──────────────┘         └──────────────┘            │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                              ORDER DOMAIN                                        │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│   ┌──────────────┐  converts   ┌──────────────┐  creates   ┌──────────────┐     │
│   │  Quotation   │────────────►│    Order     │───────────►│   Payment    │     │
│   └──────────────┘             └──────────────┘            └──────────────┘     │
│          │                            │                                          │
│          │                            │                                          │
│          ▼                            ▼                                          │
│   ┌──────────────┐             ┌──────────────┐                                  │
│   │  Quotation   │             │  OrderItem   │                                  │
│   │    Item      │             │              │                                  │
│   └──────────────┘             └──────────────┘                                  │
│                                       │                                          │
│                    ┌──────────────────┼──────────────────┐                       │
│                    │                  │                  │                       │
│                    ▼                  ▼                  ▼                       │
│             ┌──────────────┐   ┌──────────────┐   ┌──────────────┐              │
│             │ Cancellation │   │    Return    │   │   Shipment   │              │
│             └──────────────┘   └──────────────┘   └──────────────┘              │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Quotation Design

### 4.1 Quotation Lifecycle

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Requested  │────►│ UnderReview │────►│   Quoted    │────►│  Converted  │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
      │                   │                   │
      │                   │                   │
      ▼                   ▼                   ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Cancelled  │     │  Rejected   │     │   Expired   │
└─────────────┘     └─────────────┘     └─────────────┘
```

### 4.2 Quotation Entity Structure

**Quotation (Aggregate Root)**

| Field | Type | Description |
|-------|------|-------------|
| QuotationNumber | string | Unique identifier (QUO-YYYYMMDD-XXXXXXXX) |
| CustomerId | int | FK to Customer |
| RequestedByUserId | int | User who requested the quote |
| RequestDate | DateTime | When requested |
| ResponseDate | DateTime? | When admin responded |
| ValidUntil | DateTime? | Quote expiration date |
| Status | QuotationStatus | Current status |
| Currency | string | Customer's preferred currency |
| BaseCurrency | string | System base currency (TRY) |
| LockedExchangeRate | decimal? | Rate locked at quote time |
| ShippingAddressId | int | Delivery address |
| BillingAddressId | int | Invoice address |
| Subtotal | Money | Sum of item totals |
| TaxAmount | Money | Total tax |
| DiscountAmount | Money | Total discounts |
| TotalAmount | Money | Grand total |
| RespondedByUserId | int? | Admin who responded |
| InternalNotes | string? | Admin-only notes |
| CustomerNotes | string? | Notes visible to customer |
| RejectionReason | string? | If rejected |
| ConvertedToOrderId | int? | Resulting order |
| ExternalCode | string? | LOGO ERP reference |

**QuotationItem**

| Field | Type | Description |
|-------|------|-------------|
| QuotationId | int | FK to Quotation |
| ProductId | int | FK to Product |
| Sku | string | Product SKU |
| ProductName | string | Product name snapshot |
| Quantity | int | Requested quantity |
| RequestedNote | string? | Customer's note for item |
| IsQuoted | bool | Whether admin has quoted |
| UnitPrice | Money? | Quoted unit price |
| OriginalUnitPrice | Money? | Reference price (list/dealer) |
| DiscountPercent | decimal? | Applied discount |
| TaxRate | decimal | VAT rate |
| IsAvailable | bool? | Stock availability |
| AvailableQuantity | int? | Available stock |
| AvailabilityNote | string? | Lead time info |

### 4.3 Quotation Business Rules

1. **Request Phase**
   - Any authorized user can request a quotation
   - Items can be added from cart or manually
   - Customer notes can be added per item

2. **Response Phase**
   - Admin reviews and sets prices per item
   - Can mark items as unavailable with reason
   - Must set validity period (default: 7 days)
   - Exchange rate locked at response time

3. **Conversion Phase**
   - Only "Quoted" status can convert to order
   - Must be within validity period
   - Only available items are converted
   - Prices are locked from quotation

4. **Expiration**
   - Background job checks daily
   - Expired quotations cannot be converted
   - Customer can request new quote

---

## 5. Order Design

### 5.1 Order Lifecycle

```
┌─────────┐    ┌─────────────────┐    ┌──────────┐    ┌────────────┐
│  Draft  │───►│ PendingApproval │───►│ Approved │───►│ Processing │
└─────────┘    └─────────────────┘    └──────────┘    └────────────┘
                      │                     │               │
                      ▼                     ▼               ▼
               ┌──────────┐          ┌───────────┐    ┌─────────┐
               │ Rejected │          │ Partially │    │ Shipped │
               └──────────┘          │ Approved  │    └─────────┘
                                     └───────────┘         │
                                                           ▼
                                                     ┌───────────┐
                                                     │ Delivered │
                                                     └───────────┘
                                                           │
                                                           ▼
                                                     ┌───────────┐
    ┌───────────┐                                    │ Completed │
    │ Cancelled │◄───── (Can cancel before shipping) └───────────┘
    └───────────┘
```

### 5.2 Order Entity Structure

**Order (Aggregate Root)**

| Field | Type | Description |
|-------|------|-------------|
| OrderNumber | string | Unique identifier (ORD-YYYYMMDD-XXXXXXXX) |
| CustomerId | int | FK to Customer |
| CreatedByUserId | int | User who placed order |
| OrderDate | DateTime | Order creation date |
| Source | OrderSource | Direct / Quotation |
| SourceQuotationId | int? | If from quotation |
| Status | OrderStatus | Current status |
| Currency | string | Order currency |
| BaseCurrency | string | System base currency |
| ExchangeRate | decimal? | Locked at approval |
| ShippingAddressId | int | Delivery address |
| BillingAddressId | int | Invoice address |
| ShippingAddress | AddressSnapshot | Address snapshot |
| BillingAddress | AddressSnapshot | Address snapshot |
| PaymentMethod | PaymentMethod | Selected payment method |
| PaymentStatus | PaymentStatus | Payment state |
| RequiresApproval | bool | Based on rules/settings |
| ApprovedByUserId | int? | Who approved |
| ApprovedDate | DateTime? | When approved |
| RejectionReason | string? | If rejected |
| Subtotal | Money | Sum before tax |
| TaxAmount | Money | Total VAT |
| ShippingAmount | Money | Delivery cost |
| DiscountAmount | Money | Total discounts |
| TotalAmount | Money | Grand total |
| CustomerNotes | string? | Customer instructions |
| InternalNotes | string? | Admin notes |
| ExternalCode | string? | LOGO order reference |
| ExternalInvoiceCode | string? | LOGO invoice reference |
| IsSyncedToErp | bool | ERP sync status |

**OrderItem**

| Field | Type | Description |
|-------|------|-------------|
| OrderId | int | FK to Order |
| ProductId | int | FK to Product |
| Sku | string | Product SKU |
| ProductName | string | Name snapshot |
| Quantity | int | Ordered quantity |
| UnitPrice | Money | Price per unit |
| OriginalPrice | Money | Reference price |
| DiscountPercent | decimal? | Applied discount |
| DiscountAmount | Money | Discount value |
| TaxRate | decimal | VAT rate |
| TaxAmount | Money | Tax value |
| Subtotal | Money | Quantity × UnitPrice |
| TotalPrice | Money | Including tax |
| ItemStatus | OrderItemStatus | Pending/Approved/Rejected |
| RejectionReason | string? | If rejected |
| AppliedCampaignId | int? | Campaign if applied |
| QuantityShipped | int | Shipped so far |
| QuantityReturned | int | Returned so far |

**OrderStatusHistory**

| Field | Type | Description |
|-------|------|-------------|
| OrderId | int | FK to Order |
| Status | OrderStatus | Status at this point |
| ChangedByUserId | int | Who changed it |
| Description | string | Change reason/note |
| ChangedAt | DateTime | When changed |

### 5.3 Order Sources

| Source | Description | Pricing |
|--------|-------------|---------|
| **Direct** | Created from cart | Real-time pricing |
| **Quotation** | Converted from quote | Locked quotation prices |

### 5.4 Address Snapshot Strategy

Orders store address snapshots (copies) rather than references because:
- Customer may change addresses after order
- Historical orders must show original delivery address
- Audit trail requirements

---

## 6. Pricing Architecture

### 6.1 Product Price Fields

| Field | Description |
|-------|-------------|
| **ListPrice** | Retail/MSRP price (highest) |
| **Tier1Price** | Best pricing tier |
| **Tier2Price** | Second best pricing |
| **Tier3Price** | Mid-tier pricing |
| **Tier4Price** | Entry-level pricing |
| **Tier5Price** | Default/lowest tier pricing |

### 6.2 Price Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│                     PRICING PRIORITY                         │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│   1. Customer Special Price (negotiated per product)         │
│      ↓ if not found                                          │
│   2. Campaign Price (time-limited discount)                  │
│      ↓ if not found                                          │
│   3. Tier Price (based on customer's assigned tier 1-5)     │
│      ↓ if customer has no tier assigned                      │
│   4. List Price (default retail price)                       │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 6.3 Customer Tier Assignment

| Tier | Description | Typical Use Case |
|------|-------------|------------------|
| **Tier 1** | Premium dealers | Highest volume, strategic partners |
| **Tier 2** | Gold dealers | High volume dealers |
| **Tier 3** | Silver dealers | Medium volume dealers |
| **Tier 4** | Standard dealers | Regular dealers |
| **Tier 5** | Entry dealers | New or low-volume dealers |
| **No Tier** | Unassigned | Uses ListPrice |

### 6.4 Price Resolution Logic

```
GetPriceForCustomer(Product, Customer):
    
    1. Check CustomerSpecialPrice for this Product + Customer
       → If exists and valid date: return SpecialPrice
    
    2. Check Active Campaigns for this Product
       → If campaign active: return CampaignPrice
    
    3. Check Customer.PriceTier
       → If Tier 1: return Product.Tier1Price
       → If Tier 2: return Product.Tier2Price
       → If Tier 3: return Product.Tier3Price
       → If Tier 4: return Product.Tier4Price
       → If Tier 5: return Product.Tier5Price
    
    4. Default: return Product.ListPrice
```

### 6.5 Product Price Entity

| Field | Type | Description |
|-------|------|-------------|
| ProductId | int | FK to Product |
| ListPrice | Money | Retail price |
| Tier1Price | Money? | Tier 1 price (nullable) |
| Tier2Price | Money? | Tier 2 price (nullable) |
| Tier3Price | Money? | Tier 3 price (nullable) |
| Tier4Price | Money? | Tier 4 price (nullable) |
| Tier5Price | Money? | Tier 5 price (nullable) |
| Currency | string | Price currency |
| EffectiveFrom | DateTime | Price valid from |
| EffectiveTo | DateTime? | Price valid until (null = no end) |

*Note: Tier prices are nullable. If a tier price is not set, system falls back to ListPrice.*

### 6.6 Customer Special Price Entity

| Field | Type | Description |
|-------|------|-------------|
| CustomerId | int | FK to Customer |
| ProductId | int | FK to Product |
| SpecialPrice | Money | Negotiated price |
| Currency | string | Price currency |
| EffectiveFrom | DateTime | Valid from date |
| EffectiveTo | DateTime? | Valid until date |
| ApprovedByUserId | int | Who approved this price |
| Notes | string? | Reason for special price |
| IsActive | bool | Active flag |

### 6.8 Multi-Currency Handling

```
┌─────────────────────────────────────────────────────────────┐
│                  CURRENCY FLOW                               │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│   Product (Base Currency: TRY)                               │
│          │                                                   │
│          ▼                                                   │
│   Customer Preferred Currency (USD/EUR/TRY)                  │
│          │                                                   │
│          ▼                                                   │
│   Quotation/Order (Customer Currency)                        │
│          │                                                   │
│          ├── Exchange Rate Locked at Approval                │
│          │                                                   │
│          ▼                                                   │
│   Credit Impact (Converted to Base Currency)                 │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

**Exchange Rate Locking Points:**
- **Quotation**: Locked when admin responds
- **Direct Order**: Locked at approval time
- **Purpose**: Protects against currency fluctuation

---

## 7. Approval System Design

### 7.1 Approval Decision Flow

```
┌─────────────────────────────────────────────────────────────┐
│                  APPROVAL DECISION                           │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│   Order Created                                              │
│        │                                                     │
│        ▼                                                     │
│   ┌─────────────────────┐                                    │
│   │ Check System Config │──► RequireOrderApproval = false    │
│   │ (Global Setting)    │    → Auto-Approve                  │
│   └─────────────────────┘                                    │
│        │ true                                                │
│        ▼                                                     │
│   ┌─────────────────────┐                                    │
│   │ Check Customer      │──► RequireOrderApproval = false    │
│   │ Setting             │    → Auto-Approve                  │
│   └─────────────────────┘                                    │
│        │ true                                                │
│        ▼                                                     │
│   ┌─────────────────────┐                                    │
│   │ Evaluate Approval   │──► Rule matches Auto-Approve       │
│   │ Rules (Priority)    │    → Auto-Approve                  │
│   └─────────────────────┘                                    │
│        │ no auto-approve                                     │
│        ▼                                                     │
│   Requires Manual Approval                                   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 7.2 Approval Rule Types

| Rule Type | Description | Example Configuration |
|-----------|-------------|----------------------|
| **AmountThreshold** | Based on order total | Orders < 10,000 TRY auto-approve |
| **CustomerTier** | Based on customer tier (1-5) | Tier 1 & 2 always auto-approve |
| **PaymentMethod** | Based on payment type | Credit card always auto-approve |
| **CreditUtilization** | Based on credit usage | < 50% utilization auto-approve |
| **NewCustomer** | First N orders | First 3 orders require approval |
| **ProductCategory** | Specific categories | Category X requires approval |
| **TimeWindow** | Business hours | After-hours requires approval |

### 7.3 Approval Rule Entity

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Rule identifier |
| Description | string | Human-readable description |
| RuleType | ApprovalRuleType | Type of rule |
| Priority | int | Lower = higher priority |
| IsActive | bool | Enable/disable |
| RuleConfiguration | JSON | Rule parameters |
| Action | ApprovalAction | AutoApprove/RequireApproval/AutoReject |
| RequiredApproverRoleId | int? | Who can approve |

### 7.4 Partial Approval

For B2B, partial approval allows:
- Approve some items, reject others
- Customer decides whether to proceed
- Stock availability issues handled gracefully

**Customer Setting:** `AllowPartialOrderApproval` (true/false)

---

## 8. Payment Methods

### 8.1 Supported Payment Methods

| Method | Description | When to Use |
|--------|-------------|-------------|
| **CreditLimit** | Use dealer credit | Has available credit |
| **BankTransfer** | Wire transfer | Prepayment or no credit |
| **CreditCard** | Virtual POS | Immediate payment needed |
| **MailOrder** | Phone/manual card | Special situations |
| **Combined** | Credit + other | Partial credit available |

### 8.2 Payment Entity Structure

**Payment (Aggregate Root)**

| Field | Type | Description |
|-------|------|-------------|
| PaymentNumber | string | Unique reference |
| OrderId | int | FK to Order |
| CustomerId | int | FK to Customer |
| Method | PaymentMethod | Payment type |
| Amount | Money | Payment amount |
| Status | PaymentStatus | Current status |
| GatewayTransactionId | string? | POS reference |
| BankAccountId | string? | For transfers |
| TransferReference | string? | Transfer ref |
| CommissionAmount | Money? | Transaction fee |
| CommissionRate | decimal? | Fee percentage |

### 8.3 Payment Status Flow

```
┌─────────┐    ┌────────────┐    ┌───────────┐    ┌───────────┐
│ Pending │───►│ Processing │───►│ Completed │    │  Failed   │
└─────────┘    └────────────┘    └───────────┘    └───────────┘
                     │                                  ▲
                     └──────────────────────────────────┘
                                                        
                     ┌───────────┐    ┌───────────┐
                     │ Cancelled │    │ Refunded  │
                     └───────────┘    └───────────┘
```

### 8.4 Commission Management

Commissions are calculated based on:
- Payment Service Provider (PSP) rates
- Bank-specific rates
- Customer-specific negotiated rates

---

## 9. Cancellation Design

### 9.1 Cancellation Eligibility

| Order Status | Can Cancel? | Notes |
|--------------|-------------|-------|
| Draft | ✅ Yes | No impact |
| PendingApproval | ✅ Yes | No stock reserved |
| Approved | ✅ Yes | Release stock, credit |
| PartiallyApproved | ✅ Yes | Release stock, credit |
| Processing | ✅ Yes | May have partial fulfillment |
| Shipped | ❌ No | Must use Return |
| Delivered | ❌ No | Must use Return |
| Completed | ❌ No | Must use Return |
| Cancelled | ❌ No | Already cancelled |

### 9.2 OrderCancellation Entity

| Field | Type | Description |
|-------|------|-------------|
| CancellationNumber | string | Unique reference (CAN-...) |
| OrderId | int | FK to Order |
| CustomerId | int | FK to Customer |
| CancelledByUserId | int | Who initiated |
| PreviousOrderStatus | OrderStatus | Status before cancel |
| Reason | string | Cancellation reason |
| Initiator | CancellationInitiator | Customer/Admin/System |
| RequiresApproval | bool | Based on rules |
| Status | CancellationStatus | Pending/Approved/Rejected |
| RefundAmount | Money | Amount to refund |
| CreditReleased | bool | Credit restored? |
| StockReleased | bool | Stock restored? |

### 9.3 Cancellation Side Effects

When a cancellation is approved:
1. **Stock Release**: Reserved stock returned to inventory
2. **Credit Release**: Used credit restored to customer
3. **Payment Refund**: If paid, initiate refund process
4. **ERP Notification**: Sync cancellation to LOGO

---

## 10. Return Design

### 10.1 Return Lifecycle

```
┌───────────┐    ┌─────────────────┐    ┌──────────┐
│ Requested │───►│ PendingApproval │───►│ Approved │
└───────────┘    └─────────────────┘    └──────────┘
                        │                     │
                        ▼                     ▼
                 ┌──────────┐          ┌───────────┐
                 │ Rejected │          │ Partially │
                 └──────────┘          │ Approved  │
                                       └───────────┘
                                             │
                 ┌───────────────────────────┘
                 │
                 ▼
          ┌───────────┐    ┌──────────┐    ┌───────────┐
          │  Shipped  │───►│ Received │───►│ Inspected │
          └───────────┘    └──────────┘    └───────────┘
                                                 │
                                                 ▼
                                          ┌──────────┐    ┌───────────┐
                                          │ Refunded │───►│ Completed │
                                          └──────────┘    └───────────┘
```

### 10.2 Return Entity Structure

**Return (Aggregate Root)**

| Field | Type | Description |
|-------|------|-------------|
| ReturnNumber | string | Unique reference (RET-...) |
| OrderId | int | FK to Order |
| CustomerId | int | FK to Customer |
| RequestedByUserId | int | Who requested |
| RequestDate | DateTime | When requested |
| Reason | ReturnReason | Why returning |
| ReasonDescription | string | Detailed reason |
| Status | ReturnStatus | Current status |
| RequiresApproval | bool | Based on settings |
| ReturnShippingCarrier | string? | Carrier name |
| ReturnTrackingNumber | string? | Tracking code |
| ReceivedDate | DateTime? | When received |
| InspectedDate | DateTime? | When inspected |
| InspectionNotes | string? | Inspection results |
| RefundMethod | RefundMethod | Credit/BankTransfer/OriginalMethod |
| RefundAmount | Money | Total refund |
| RefundDate | DateTime? | When refunded |

**ReturnItem**

| Field | Type | Description |
|-------|------|-------------|
| ReturnId | int | FK to Return |
| OrderItemId | int | FK to OrderItem |
| ProductId | int | FK to Product |
| Quantity | int | Return quantity |
| UnitPrice | Money | Original price |
| RefundAmount | Money | Item refund |
| RequestedCondition | ReturnItemCondition | Customer claims |
| ActualCondition | ReturnItemCondition? | After inspection |
| Status | ReturnItemStatus | Pending/Approved/Rejected |
| RestockedToInventory | bool | Back in stock? |

### 10.3 Return Reasons

| Reason | Description | Typical Refund |
|--------|-------------|----------------|
| **Defective** | Product doesn't work | 100% |
| **WrongItem** | Received wrong product | 100% |
| **Damaged** | Damaged in shipping | 100% |
| **NotAsDescribed** | Doesn't match listing | 100% |
| **ChangedMind** | Buyer's remorse | May deduct |
| **BetterPrice** | Found cheaper | May deduct |
| **NoLongerNeeded** | Requirements changed | May deduct |

### 10.4 Return Item Conditions & Refund Adjustment

| Condition | Description | Refund % |
|-----------|-------------|----------|
| **New** | Unopened, original packaging | 100% |
| **LikeNew** | Opened but unused | 100% |
| **Good** | Minor signs of handling | 90% |
| **Fair** | Visible wear | 70% |
| **Damaged** | Significant damage | 50% |
| **Defective** | Manufacturer defect | 100% |

---

## 11. Enumerations

### 11.1 Quotation Enums

```
QuotationStatus:
  - Requested
  - UnderReview
  - Quoted
  - Converted
  - Rejected
  - Expired
  - Cancelled
```

### 11.2 Order Enums

```
OrderStatus:
  - Draft
  - PendingApproval
  - Approved
  - PartiallyApproved
  - Rejected
  - Processing
  - Shipped
  - Delivered
  - Completed
  - Cancelled

OrderSource:
  - Direct
  - Quotation

OrderItemStatus:
  - Pending
  - Approved
  - Rejected
  - Cancelled
```

### 11.3 Payment Enums

```
PaymentMethod:
  - CreditLimit
  - BankTransfer
  - CreditCard
  - MailOrder
  - Combined

PaymentStatus:
  - Pending
  - Processing
  - Completed
  - Failed
  - Cancelled
  - Refunded
```

### 11.4 Approval Enums

```
ApprovalRuleType:
  - AmountThreshold
  - CustomerTier
  - PaymentMethod
  - CreditUtilization
  - NewCustomer
  - ProductCategory
  - TimeWindow

ApprovalAction:
  - AutoApprove
  - RequireApproval
  - AutoReject

ApprovalStepStatus:
  - Pending
  - Approved
  - Rejected
  - Skipped
```

### 11.5 Cancellation Enums

```
CancellationInitiator:
  - Customer
  - Admin
  - System

CancellationStatus:
  - PendingApproval
  - Approved
  - Rejected
```

### 11.6 Return Enums

```
ReturnStatus:
  - Requested
  - PendingApproval
  - Approved
  - PartiallyApproved
  - Rejected
  - Shipped
  - Received
  - Inspected
  - Refunded
  - Completed
  - Cancelled

ReturnReason:
  - Defective
  - WrongItem
  - Damaged
  - NotAsDescribed
  - ChangedMind
  - BetterPrice
  - NoLongerNeeded
  - Other

ReturnItemCondition:
  - New
  - LikeNew
  - Good
  - Fair
  - Damaged
  - Defective

ReturnItemStatus:
  - Pending
  - Approved
  - Rejected

RefundMethod:
  - Credit          (Add to customer credit)
  - BankTransfer    (Wire to customer bank)
  - OriginalMethod  (Refund via original payment)
```

### 11.7 Pricing Enums

```
PriceTier:
  - Tier1    (Best pricing)
  - Tier2
  - Tier3
  - Tier4
  - Tier5    (Entry-level pricing)
```

### 11.8 Note Enums

```
NoteType:
  - System    (Auto-generated by system)
  - Admin     (Created by admin user)
```

---

## 12. Notes System

### 12.1 Overview

Both Quotation and Order aggregates support a flexible notes system allowing system-generated and admin-created notes with visibility control.

### 12.2 Note Entity Structure

**QuotationNote**

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| QuotationId | int | FK to Quotation |
| Content | string | Note text (max 2000 chars) |
| NoteType | NoteType | System / Admin |
| CreatedByUserId | int? | Null if system-generated |
| CreatedAt | DateTime | When created |
| IsVisibleToCustomer | bool | Show to customer? |
| RelatedEntityType | string? | Optional: "Item", "Payment", etc. |
| RelatedEntityId | int? | Optional: Related entity ID |

**OrderNote**

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| OrderId | int | FK to Order |
| Content | string | Note text (max 2000 chars) |
| NoteType | NoteType | System / Admin |
| CreatedByUserId | int? | Null if system-generated |
| CreatedAt | DateTime | When created |
| IsVisibleToCustomer | bool | Show to customer? |
| RelatedEntityType | string? | Optional: "Item", "Shipment", "Payment", etc. |
| RelatedEntityId | int? | Optional: Related entity ID |

### 12.3 Note Type Enum

```
NoteType:
  - System    (Auto-generated by system events)
  - Admin     (Created by admin/sales user)
```

### 12.4 Usage Examples

| Scenario | NoteType | IsVisibleToCustomer | Example Content |
|----------|----------|---------------------|-----------------|
| Price override explanation | Admin | ✅ Yes | "Special pricing applied per agreement #123" |
| Internal credit concern | Admin | ❌ No | "Customer approaching credit limit, monitor closely" |
| Auto-approval reason | System | ✅ Yes | "Order auto-approved: Amount under threshold" |
| ERP sync failure | System | ❌ No | "ERP sync failed at 14:32, retry scheduled" |
| Shipping delay notice | Admin | ✅ Yes | "Item X delayed 2 days due to supplier stock" |
| Fraud check flag | System | ❌ No | "Order flagged for review: New customer, high value" |
| Partial approval reason | Admin | ✅ Yes | "Item Y rejected: Currently out of stock" |
| Payment reminder | System | ✅ Yes | "Payment pending. Bank transfer details sent." |

### 12.5 System-Generated Note Triggers

**Quotation Notes (Auto-Generated)**

| Trigger | Content Template | Visible |
|---------|------------------|---------|
| Quote created | "Quotation requested by {UserName}" | No |
| Quote responded | "Quotation prepared by {AdminName}, valid until {Date}" | Yes |
| Quote expired | "Quotation expired on {Date}" | Yes |
| Quote converted | "Converted to Order {OrderNumber}" | Yes |

**Order Notes (Auto-Generated)**

| Trigger | Content Template | Visible |
|---------|------------------|---------|
| Order created | "Order placed by {UserName}" | No |
| Auto-approved | "Order auto-approved: {RuleName}" | Yes |
| Manual approval | "Order approved by {AdminName}" | Yes |
| Partial approval | "Order partially approved. {N} items rejected." | Yes |
| Rejection | "Order rejected: {Reason}" | Yes |
| Payment received | "Payment of {Amount} received via {Method}" | Yes |
| Shipment created | "Shipment {TrackingNumber} created via {Carrier}" | Yes |
| Status change | "Status changed from {Old} to {New}" | No |
| ERP sync success | "Synced to ERP: {ExternalCode}" | No |
| ERP sync failure | "ERP sync failed: {Error}" | No |

### 12.6 Access Control

| Role | Can Create | Can View Internal | Can Edit | Can Delete |
|------|------------|-------------------|----------|------------|
| System | ✅ | N/A | ❌ | ❌ |
| Super Admin | ✅ | ✅ | ✅ Own | ✅ |
| Sales Manager | ✅ | ✅ | ✅ Own | ❌ |
| Customer Service | ✅ | ✅ | ✅ Own | ❌ |
| Dealer User | ❌ | ❌ | ❌ | ❌ |

*Dealer users can only view notes where `IsVisibleToCustomer = true`*

### 12.7 Related Entity Linking

Notes can optionally link to related entities for context:

| RelatedEntityType | Use Case |
|-------------------|----------|
| "Item" | Note about specific order/quotation item |
| "Shipment" | Note about specific shipment |
| "Payment" | Note about specific payment |
| "Return" | Note about related return |
| "Cancellation" | Note about cancellation |
| "ApprovalStep" | Note about approval decision |

### 12.8 Entity Relationships

```
┌──────────────┐          ┌──────────────────┐
│  Quotation   │ 1──────n │  QuotationNote   │
└──────────────┘          └──────────────────┘
                                   │
                                   │ created by
                                   ▼
                          ┌──────────────────┐
                          │      User        │
                          │   (nullable)     │
                          └──────────────────┘
                                   ▲
                                   │ created by
                                   │
┌──────────────┐          ┌──────────────────┐
│    Order     │ 1──────n │    OrderNote     │
└──────────────┘          └──────────────────┘
```

### 12.9 API Considerations

**Endpoints (B2B API - Admin)**
- `GET /api/quotations/{id}/notes` - List all notes
- `POST /api/quotations/{id}/notes` - Add note
- `PUT /api/quotations/{id}/notes/{noteId}` - Update own note
- `GET /api/orders/{id}/notes` - List all notes
- `POST /api/orders/{id}/notes` - Add note
- `PUT /api/orders/{id}/notes/{noteId}` - Update own note

**Endpoints (B2B API - Dealer)**
- `GET /api/quotations/{id}/notes` - List visible notes only
- `GET /api/orders/{id}/notes` - List visible notes only

**Query Filters**
- `?visibleOnly=true` - Only customer-visible notes
- `?noteType=System|Admin` - Filter by type
- `?relatedEntityType=Item&relatedEntityId=123` - Filter by related entity

---

## 13. Domain Events

### 13.1 Quotation Events

| Event | Trigger | Handlers |
|-------|---------|----------|
| QuotationRequestedEvent | Quote created | Notify admin, create task |
| QuotationRespondedEvent | Admin responds | Notify customer (email/SMS) |
| QuotationRejectedEvent | Admin rejects | Notify customer |
| QuotationExpiredEvent | Validity passed | Log, notify customer |
| QuotationConvertedToOrderEvent | Converted | Create order, update stats |

### 13.2 Order Events

| Event | Trigger | Handlers |
|-------|---------|----------|
| OrderCreatedEvent | Order created | Reserve stock (if auto-approved) |
| OrderApprovedEvent | Approved | Reserve stock, use credit, notify, sync ERP |
| OrderPartiallyApprovedEvent | Partial approval | Reserve partial stock, notify |
| OrderRejectedEvent | Rejected | Notify customer |
| OrderProcessingStartedEvent | Start processing | Update ERP status |
| OrderShippedEvent | Shipped | Notify customer, send tracking |
| OrderDeliveredEvent | Delivered | Update status |
| OrderCompletedEvent | Completed | Update stats, trigger review request |
| OrderCancelledEvent | Cancelled | Release stock/credit, refund |

### 13.3 Payment Events

| Event | Trigger | Handlers |
|-------|---------|----------|
| PaymentInitiatedEvent | Payment started | Create transaction record |
| PaymentCompletedEvent | Payment success | Update order, credit transaction |
| PaymentFailedEvent | Payment failed | Notify, retry logic |
| PaymentRefundedEvent | Refund processed | Update credit, notify |

### 13.4 Return Events

| Event | Trigger | Handlers |
|-------|---------|----------|
| ReturnRequestedEvent | Return created | Notify admin |
| ReturnApprovedEvent | Approved | Generate shipping label |
| ReturnReceivedEvent | Received at warehouse | Queue for inspection |
| ReturnInspectedEvent | Inspection complete | Calculate final refund |
| ReturnRefundedEvent | Refund processed | Update credit/payment |

---

## 14. Integration Points

### 14.1 ERP Integration (LOGO)

| Operation | Direction | Trigger |
|-----------|-----------|---------|
| Order Creation | B2B → ERP | Order approved |
| Order Update | B2B → ERP | Status changes |
| Order Cancellation | B2B → ERP | Cancellation approved |
| Invoice Creation | ERP → B2B | Invoice generated |
| Return Creation | B2B → ERP | Return approved |
| Stock Update | ERP → B2B | Reservation/release |

### 14.2 Notification Integration

| Channel | Events |
|---------|--------|
| Email | Order confirmation, shipping, quotes |
| SMS | Order status, payment confirmation |
| Push | Mobile app notifications |

### 14.3 Payment Gateway Integration

| Gateway | Purpose |
|---------|---------|
| Paynet | Virtual POS, multiple banks |
| Bank APIs | Transfer verification |

---

## 15. Database Schema Summary

### 15.1 Core Tables

```
Quotations
├── QuotationItems
├── QuotationNotes
│
Orders
├── OrderItems
├── OrderStatusHistory
├── OrderApprovalSteps
├── OrderNotes
│
Payments
│
OrderCancellations
├── OrderCancellationItems
│
Returns
├── ReturnItems
├── ReturnStatusHistory
│
ApprovalRules
```

### 15.2 Key Indexes

| Table | Index | Purpose |
|-------|-------|---------|
| Quotations | IX_CustomerId_Status | Customer quote listing |
| Quotations | IX_ValidUntil | Expiration job |
| QuotationNotes | IX_QuotationId | Notes for quotation |
| QuotationNotes | IX_IsVisibleToCustomer | Customer-visible filter |
| Orders | IX_CustomerId_Status | Customer order listing |
| Orders | IX_OrderDate | Date range queries |
| Orders | IX_ExternalCode | ERP sync lookups |
| OrderItems | IX_OrderId_ProductId | Order item queries |
| OrderNotes | IX_OrderId | Notes for order |
| OrderNotes | IX_IsVisibleToCustomer | Customer-visible filter |
| Payments | IX_OrderId | Order payments |
| Returns | IX_OrderId | Order returns |

---

## 16. Business Rules Summary

### 16.1 Pricing Rules

1. Price hierarchy: Special → Campaign → Tier → Dealer → List
2. Dealer price must be ≤ List price
3. Prices locked at quotation response or order approval
4. Exchange rates locked to prevent fluctuation impact

### 16.2 Approval Rules

1. System setting overrides all if disabled
2. Customer setting adds additional requirement
3. Rules evaluated in priority order
4. First matching rule determines action
5. Partial approval requires customer setting enabled

### 16.3 Credit Rules

1. Available Credit = Limit - Used - Pending Orders
2. Credit reserved at approval
3. Credit released on cancellation/return
4. Zero credit limit = prepayment required

### 16.4 Stock Rules

1. Stock reserved at approval
2. Stock released on cancellation
3. Stock restored on return (after inspection)
4. Insufficient stock can trigger partial approval

### 16.5 Cancellation Rules

1. Can cancel before shipping
2. May require approval based on settings
3. Auto-release stock and credit on approval
4. Refund processed based on payment method

### 16.6 Return Rules

1. Can return after delivery
2. Return window configurable (e.g., 14 days)
3. Refund adjusted based on condition
4. Defective items get full refund regardless

---

## 17. Next Steps

1. **Review & Approval**: Stakeholder review of this design
2. **Entity Implementation**: Create domain entities per this spec
3. **Repository Layer**: Implement data access
4. **Application Services**: Business logic implementation
5. **API Endpoints**: B2B API and Integration API
6. **Testing**: Unit and integration tests
7. **Frontend Integration**: UI components

---

**Document Version**: 1.0  
**Author**: Claude (Design Assistant)  
**Review Status**: Draft - Pending Review
