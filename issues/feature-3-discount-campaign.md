### Title
Implementing Campaign-based Product Discount Feature

### Description
B2B managers need to create time-limited discount campaigns for products. Discounts can target specific products, categories, brands, or apply to all products. Campaigns have budget controls and can be restricted to specific customers or customer tiers. This feature is part of the Campaign & Promotion module.

### User Story
As a B2B manager
I want to create discount campaigns for products with flexible targeting and budget controls
So that I can run promotional activities to increase sales within controlled limits

### Requirements

**Campaign Management**
- [ ] A campaign has a name, description, start date, and end date
- [ ] Campaign statuses: Draft, Scheduled, Active, Paused, Ended, Cancelled
- [ ] Campaign has a priority value (higher priority wins when multiple campaigns apply)
- [ ] Campaign can be activated, paused, resumed, and cancelled
- [ ] Scheduled campaigns automatically become active when start date is reached
- [ ] Active campaigns automatically end when end date passes
- [ ] Campaign should auto-pause when budget is exhausted

**Discount Rules**
- [ ] Each campaign can have multiple discount rules
- [ ] Discount types: Percentage (e.g., 15% off) and Fixed Amount (e.g., ₺100 off)
- [ ] Discounts apply to customer's resolved price (Special Price > Tier Price > Dealer Price)
- [ ] Percentage discounts can have a maximum discount cap (e.g., max ₺500 off)
- [ ] Discounts can require minimum quantity (e.g., buy 5+ to get discount)

**Product Targeting (Scope)**
- [ ] All products
- [ ] Specific products (select individual products)
- [ ] Categories (includes all products in category and its subcategories)
- [ ] Brands (all products of selected brands)
- [ ] When product matches multiple scopes, priority is: Product > Brand > Category > All

**Customer Targeting**
- [ ] All customers
- [ ] Specific customers (select individual customers)
- [ ] Customer tiers (A, B, C tier customers)

**Budget & Limit Controls**
- [ ] Maximum total discount amount for campaign (e.g., max ₺100,000 total)
- [ ] Maximum total usage count (e.g., max 1,000 orders)
- [ ] Maximum discount amount per customer (e.g., max ₺1,000 per customer)
- [ ] Maximum usage count per customer (e.g., max 5 orders per customer)

**Discount Resolution**
- [ ] When multiple campaigns apply to same product, best discount wins (highest savings)
- [ ] If savings are equal, higher priority campaign wins
- [ ] Budget limits are checked before applying discount
- [ ] Partial discount can be applied if remaining budget is less than full discount

**Usage Tracking**
- [ ] Track discount usage when order is placed
- [ ] Reverse usage when order is cancelled
- [ ] Track total discount given and usage count per campaign
- [ ] Track per-customer usage for customer-level limits

**ERP Integration**
- [ ] Campaigns can be synced from LOGO (external code, external id)
- [ ] Campaign sync endpoint for Integration API
- [ ] Track last synced timestamp

### Business Rules
- Discounts always apply to customer's resolved price, not list price
- Customer always gets the better price (discount cannot make price higher)
- Discount amount cannot exceed the product price (no negative prices)
- Cannot modify discount rules of an active campaign (must pause first)
- Campaign must have at least one discount rule to be activated

### Sample Discount Scenarios
| Scenario | Configuration |
|----------|---------------|
| Category-wide sale | 10% off all "Networking" category products |
| Brand promotion | 15% off all "Cisco" products, max ₺500 per item |
| Flash sale | 20% off specific SKUs for 24 hours |
| Tier exclusive | Extra 5% for Tier A customers only |
| Volume discount | 10% off when buying 10+ units |
| Budget-controlled | 15% off until ₺50,000 total discount given |

### Admin Panel Features
- Campaign list with status, date range, and usage stats
- Campaign create/edit form with date picker
- Discount rule builder with product/category/brand selector
- Customer targeting selector
- Campaign performance dashboard

### Notes
- Create entity classes, repository classes, and migration files for new tables
- Follow the project's database standards and Clean Architecture patterns
- Campaign status transitions should be handled via domain methods
- Background job for auto-activation and auto-ending of campaigns will be implemented later
- Discount resolution service should support batch queries for product listings