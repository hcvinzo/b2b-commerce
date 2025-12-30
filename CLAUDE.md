# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

B2B E-Commerce Platform following Clean Architecture principles. A .NET 10 backend API with PostgreSQL (Supabase) for dealer portal, admin panel, and ERP integrations.

## Build & Run Commands

### Backend
```bash
# Build entire solution (from backend/)
cd backend && dotnet build

# Run API with hot reload
dotnet run --project backend/src/B2BCommerce.Backend.API

# Run all tests
cd backend && dotnet test

# Run specific test project
dotnet test backend/tests/B2BCommerce.Backend.Domain.Tests

# Run single test by name
dotnet test backend/tests/B2BCommerce.Backend.Domain.Tests --filter "FullyQualifiedName~TestMethodName"

# EF Core migrations (from backend/)
dotnet ef migrations add <MigrationName> --project src/B2BCommerce.Backend.Infrastructure --startup-project src/B2BCommerce.Backend.API

# Apply migrations
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update --project src/B2BCommerce.Backend.Infrastructure --startup-project src/B2BCommerce.Backend.API
```

### Frontend (Dealer Portal)
```bash
# Development
cd frontend && npm run dev

# Build for production
npm run build

# Build for test environment (uses .env.test)
npm run build:test

# Output: frontend/out/ (static export)
```

### Admin Panel
```bash
# Development
cd admin && npm run dev

# Build for production
cd admin && npm run build
```

---

## Dealer Portal (Frontend) Architecture

**Tech Stack**: Next.js 15, React 19, TypeScript, TailwindCSS, shadcn/ui, Zustand (state), React Hook Form + Zod

### Project Structure

```
frontend/src/
├── app/
│   ├── (auth)/                    # Auth route group (public pages)
│   │   ├── login/page.tsx
│   │   └── register/
│   │       ├── step-1/page.tsx    # Contact Person
│   │       ├── step-2/page.tsx    # Business Info
│   │       ├── step-3/page.tsx    # Operational Details
│   │       └── step-4/page.tsx    # Banking & Documents
│   ├── (dashboard)/               # Dashboard route group (protected)
│   └── layout.tsx
├── components/
│   └── ui/
│       ├── composite-attribute-input.tsx  # Dynamic composite fields
│       ├── single-select-attribute.tsx    # Dynamic single select
│       ├── multi-select-attribute.tsx     # Dynamic multi checkboxes
│       ├── geo-location-select.tsx        # Cascading location dropdowns
│       └── FileUpload.tsx
├── stores/
│   └── registrationStore.ts       # Zustand multi-step form state
├── lib/
│   ├── api.ts                     # API client functions
│   └── validations/
│       └── registration.schema.ts # Zod schemas for all steps
└── types/
    └── index.ts                   # TypeScript interfaces
```

### Multi-Step Registration System

The dealer registration is a 4-step form using Zustand for state persistence across steps.

**State Management** (`stores/registrationStore.ts`):
```typescript
interface RegistrationState {
  currentStep: number
  contactPerson: Partial<ContactPerson>      // Step 1
  businessInfo: Partial<BusinessInfo>        // Step 2
  operationalDetails: Partial<OperationalDetails>  // Step 3
  bankingDocuments: Partial<BankingDocuments>     // Step 4
  // Actions
  setContactPerson: (data) => void
  setBusinessInfo: (data) => void
  setOperationalDetails: (data) => void
  setBankingDocuments: (data) => void
  reset: () => void
}
```

### Dynamic Attribute Components

The registration forms use dynamic attributes loaded from the API. This allows form fields to be configured in the database.

**CompositeAttributeInput** (`components/ui/composite-attribute-input.tsx`):
- Renders a table with dynamic columns based on child attributes
- Supports `text`, `number`, `tc_kimlik`, and `select` field types
- Used for multi-row data entry (e.g., bank accounts, collaterals, business partners)

```tsx
interface CompositeAttributeField {
  code: string
  name: string
  type: 'text' | 'number' | 'tc_kimlik' | 'select'
  placeholder?: string
  required?: boolean
  min?: number
  max?: number
  maxLength?: number
  options?: SelectOption[]  // For select type
}

// Usage
<CompositeAttributeInput
  title="Banka Hesap Bilgileri"
  fields={bankAccountFields}  // Loaded from API
  values={bankAccountValues}
  onChange={setBankAccountValues}
  minRows={6}
  maxRows={10}
  addButtonText="Banka Hesabı Ekle"
/>
```

**SingleSelectAttribute** (`components/ui/single-select-attribute.tsx`):
- Loads attribute options by code from API
- Renders as dropdown select
- Used for single-choice fields (e.g., personel_sayisi, isletme_yapisi)

```tsx
<SingleSelectAttribute
  attributeCode="personel_sayisi"
  value={field.value}
  onChange={field.onChange}
  label="Personel Sayısı"
  required
/>
```

**MultiSelectAttribute** (`components/ui/multi-select-attribute.tsx`):
- Loads attribute options by code from API
- Renders as checkbox grid
- Configurable column layout (1-4 columns)

```tsx
<MultiSelectAttribute
  attributeCode="satilan_urun_kategorileri"
  value={field.value}
  onChange={field.onChange}
  label="Ürün Kategorileri"
  columns={3}
/>
```

### Loading Attributes from API

**Pattern for loading composite attributes**:
```typescript
// Load parent attribute by code
const parentAttr = await getAttributeByCode('banka_hesap_bilgileri')
setTitle(parentAttr.name)

// Load child attributes
const children = await getChildAttributes(parentAttr.id)
const fields = children
  .sort((a, b) => a.displayOrder - b.displayOrder)
  .map((attr) => ({
    code: attr.code,
    name: attr.name,
    type: mapAttributeTypeToFieldType(attr.type, attr.isList),
    placeholder: attr.name,
    required: attr.isRequired,
    options: attr.predefinedValues?.map((v) => ({
      value: v.value,
      label: v.displayText || v.value,
    })),
  }))
```

**Attribute type mapping**:
```typescript
function mapAttributeTypeToFieldType(type: number | string, isList?: boolean) {
  const typeValue = typeof type === 'string' ? type.toLowerCase() : type
  if (typeValue === 2 || typeValue === 3 || typeValue === 'select' || isList) return 'select'
  if (typeValue === 1 || typeValue === 'number') return 'number'
  if (typeValue === 'tc_kimlik') return 'tc_kimlik'
  return 'text'
}
```

### Registration Steps & Attribute Codes

| Step | Section | Attribute Code | Type |
|------|---------|---------------|------|
| 2 | Yetkililer & Ortaklar | `yetkili_ve_ortaklar` | Composite (multi-row) |
| 3 | Personel Sayısı | `personel_sayisi` | Single select |
| 3 | İşletme Yapısı | `isletme_yapisi` | Single select |
| 3 | Ciro | `ciro` | Composite (single-row) |
| 3 | Müşteri Kitlesi | `musteri_kitlesi` | Composite (single-row) |
| 3 | İş Ortakları | `is_ortaklari` | Composite (multi-row) |
| 3 | Ürün Kategorileri | `satilan_urun_kategorileri` | Multi select |
| 3 | Çalışma Koşulları | `calisma_kosullari` | Multi select |
| 4 | Banka Hesapları | `banka_hesap_bilgileri` | Composite (multi-row) |
| 4 | Teminatlar | `teminatlar` | Composite (multi-row) |

### Document Upload

Documents are uploaded to S3 during registration and stored in `Customer.DocumentUrls` as JSON:

```typescript
interface DocumentUrl {
  document_type: string  // e.g., 'taxCertificate'
  file_type: string      // e.g., 'application/pdf'
  file_url: string       // S3 URL
  file_name: string
  file_size: number
}

// DealerRegistrationDto
{
  // ... other fields
  documentUrls: JSON.stringify(documentUrls)
}
```

### API Functions

**Key API functions for registration** (`lib/api.ts`):
```typescript
// Get attribute definition by code (AllowAnonymous)
getAttributeByCode(code: string): Promise<AttributeDefinition>

// Get child attributes of composite attribute (AllowAnonymous)
getChildAttributes(parentId: string): Promise<AttributeDefinition[]>

// Register dealer
registerDealer(data: DealerRegistrationDto): Promise<RegistrationResponse>

// Upload document during registration
uploadRegistrationDocument(file: File): Promise<FileUploadResponse>
```

---

## Admin Panel Architecture

**Tech Stack**: Next.js 16, React 19, TypeScript, TailwindCSS, shadcn/ui, React Query (TanStack Query)

### Project Structure

```
admin/src/
├── app/
│   ├── (auth)/                    # Auth route group (login, forgot-password)
│   │   ├── login/page.tsx
│   │   └── layout.tsx
│   ├── (dashboard)/               # Dashboard route group (protected)
│   │   ├── layout.tsx             # Sidebar + header layout
│   │   ├── categories/page.tsx
│   │   ├── products/page.tsx
│   │   ├── customers/page.tsx
│   │   └── orders/page.tsx
│   └── layout.tsx                 # Root layout (providers)
├── components/
│   ├── ui/                        # shadcn/ui components
│   │   ├── button.tsx
│   │   ├── dialog.tsx
│   │   ├── tree-select.tsx        # Custom hierarchical dropdown
│   │   └── ...
│   ├── forms/                     # Form components
│   │   ├── category-form.tsx
│   │   └── product-form.tsx
│   └── layout/                    # Layout components
│       ├── sidebar.tsx
│       └── header.tsx
├── hooks/                         # React Query hooks
│   ├── use-categories.ts
│   ├── use-products.ts
│   └── use-auth.ts
├── lib/
│   ├── api/                       # API client functions
│   │   ├── client.ts              # Axios instance
│   │   ├── categories.ts
│   │   └── products.ts
│   └── validations/               # Zod schemas
│       ├── category.ts
│       └── product.ts
└── types/
    └── entities.ts                # TypeScript interfaces
```

### Field Naming Conventions (Backend ↔ Frontend)

**IMPORTANT**: Backend uses PascalCase in C# which becomes camelCase in JSON responses. However, some field names differ:

| Backend (C#) | Backend (JSON) | Frontend Form | Notes |
|--------------|----------------|---------------|-------|
| `ParentCategoryId` | `parentCategoryId` | `parentId` | Form uses shorter name, map when submitting |
| `SubCategories` | `subCategories` | `children` | Tree structure, requires mapping |

**Example - Category Form Submission**:
```typescript
// Form schema uses parentId
const categorySchema = z.object({
  name: z.string(),
  parentId: z.string().optional(),  // Form field name
  // ...
});

// Map to backend field when submitting
const handleFormSubmit = async (data: CategoryFormData) => {
  await createCategory.mutateAsync({
    name: data.name,
    parentCategoryId: data.parentId || undefined,  // Backend expects parentCategoryId
    // ...
  });
};
```

**Example - Category Tree Mapping**:
```typescript
// Backend response
interface CategoryTreeDto {
  id: string;
  name: string;
  subCategories?: CategoryTreeDto[];  // Backend field name
}

// Frontend interface
interface Category {
  id: string;
  name: string;
  children?: Category[];  // Frontend field name
}

// Mapping function in API layer
function mapCategoryTree(dto: CategoryTreeDto): Category {
  return {
    id: dto.id,
    name: dto.name,
    children: dto.subCategories?.map(mapCategoryTree),  // Map subCategories → children
  };
}
```

### React Query Patterns

**Hooks Location**: `admin/src/hooks/use-{entity}.ts`

```typescript
// Example: use-categories.ts
export function useCategories() {
  return useQuery({
    queryKey: ["categories"],
    queryFn: getCategories,
  });
}

export function useCreateCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
    },
  });
}
```

### API Client Pattern

**Location**: `admin/src/lib/api/{entity}.ts`

```typescript
// Categories API example
const CATEGORIES_BASE = "/categories";

export async function getCategories(): Promise<Category[]> {
  const response = await apiClient.get<CategoryTreeDto[]>(`${CATEGORIES_BASE}/tree?activeOnly=false`);
  return response.data.map(mapCategoryTree);  // Map backend → frontend
}

export async function createCategory(data: CreateCategoryDto): Promise<Category> {
  const response = await apiClient.post<Category>(CATEGORIES_BASE, data);
  return response.data;
}
```

### Form Validation with Zod

**Location**: `admin/src/lib/validations/{entity}.ts`

```typescript
export const categorySchema = z.object({
  name: z.string().min(2, "Name must be at least 2 characters").max(100),
  description: z.string().max(500).optional(),
  parentId: z.string().optional(),
  imageUrl: z.string().url("Must be a valid URL").optional().or(z.literal("")),
  displayOrder: z.number().int().min(0).optional(),
  isActive: z.boolean().optional(),
});

export type CategoryFormData = z.infer<typeof categorySchema>;
```

### shadcn/ui Component Notes

**Command/Combobox Scrolling**: Do NOT wrap CommandList with ScrollArea - it has built-in scrolling:
```tsx
// ✅ CORRECT - Use max-h on CommandList
<CommandList className="max-h-[300px]">
  <CommandGroup>{items}</CommandGroup>
</CommandList>

// ❌ WRONG - Double scrollbar
<CommandList>
  <ScrollArea className="h-[300px]">
    <CommandGroup>{items}</CommandGroup>
  </ScrollArea>
</CommandList>
```

**TreeSelect Component**: Custom hierarchical dropdown for parent selection:
- Location: `admin/src/components/ui/tree-select.tsx`
- Renders categories as indented tree in Command/Popover
- Supports `excludeId` to prevent selecting self as parent
- Search within dropdown supported

### List Page Pattern (Tables with Card Wrapper)

All list pages should wrap content in a Card component with filters, table, and pagination inside:

```tsx
// ✅ CORRECT - List page structure
<div className="space-y-6">
  {/* Page Header */}
  <div className="flex items-center justify-between">
    <div>
      <h1 className="text-3xl font-bold tracking-tight">Page Title</h1>
      <p className="text-muted-foreground">Description</p>
    </div>
    <Button>Add New</Button>
  </div>

  {/* Card with content */}
  <Card>
    <CardHeader className="pb-4">
      <CardTitle>All Items</CardTitle>
      <CardDescription>Browse and manage items</CardDescription>
    </CardHeader>
    <CardContent>
      {/* Filters */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center mb-6">
        {/* Search and filter controls */}
      </div>

      {/* Table */}
      <div className="rounded-md border">
        <Table>...</Table>
      </div>

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between mt-4">...</div>
      )}
    </CardContent>
  </Card>
</div>
```

**Row click navigation pattern**:
```tsx
<TableBody>
  {data.items.map((item) => (
    <TableRow
      key={item.id}
      className="cursor-pointer"
      onClick={() => router.push(`/entities/${item.id}`)}
    >
      <TableCell>{item.name}</TableCell>
      {/* ... other cells ... */}
      <TableCell onClick={(e) => e.stopPropagation()}>
        <DropdownMenu>
          {/* Dropdown menu content */}
        </DropdownMenu>
      </TableCell>
    </TableRow>
  ))}
</TableBody>
```

**Key points**:
- Wrap filters, table, and pagination in `Card` > `CardContent`
- Add `cursor-pointer` class to `TableRow`
- Add `onClick` handler to navigate to detail page
- Add `onClick={(e) => e.stopPropagation()}` to the dropdown menu cell to prevent row click when using dropdown
- Use `useRouter` from `next/navigation` for navigation

**Pages following this pattern**:
- `/products` - Products list
- `/customers` - Customers list
- `/admin-users` - Admin users list
- `/roles` → `/roles/[id]`
- `/api-clients` → `/api-clients/[id]`

### Detail Page Pattern (Tabbed Interface)

Entity detail pages use a tabbed interface for managing different aspects:

```
/admin/src/app/(dashboard)/{entity}/[id]/page.tsx
```

**Common structure**:
- Header with entity info and back button
- Info cards showing key details
- Tabs component with different management sections

**Example - Admin User Detail Page Tabs**:
- Overview (edit user info form)
- Roles (manage user roles with checkboxes)
- Login History (view external login providers)
- Claims (manage direct user claims)

**Example - Role Detail Page Tabs**:
- Overview (edit role info)
- Permissions (checkbox grid by category)
- Users (list and manage users in role)

**Component organization**:
```
/admin/src/components/{entity}/
├── user-roles-editor.tsx      # Manage roles for a user
├── user-login-history.tsx     # Display login providers
├── user-claims-editor.tsx     # Add/remove claims
└── ...
```

### Extended Form Components (Standard)

**IMPORTANT**: Use the extended `-ext` components for all form inputs in admin forms. These components support an optional `info` prop that displays a tooltip icon on the left side of the input.

| Base Component | Extended Component | Location |
|----------------|-------------------|----------|
| `Input` | `InputExt` | `@/components/ui/input-ext` |
| `Select` | `SelectExt` | `@/components/ui/select-ext` |
| `TreeSelect` | `TreeSelectExt` | `@/components/ui/tree-select-ext` |
| `TreeMultiSelect` | `TreeMultiSelectExt` | `@/components/ui/tree-multi-select-ext` |

**Pattern**: When `info` prop is provided, the component wraps itself with `InputGroup` and shows an info icon with tooltip. When `info` is not provided, it renders the base component without any wrapper.

**Usage Examples**:
```tsx
// Input with info tooltip
<InputExt
  info="SKU must be unique across all products"
  placeholder="Enter SKU"
  {...field}
/>

// Select with info tooltip
<SelectExt
  info="Determines which attributes are available"
  placeholder="Select product type"
  value={field.value}
  onValueChange={field.onChange}
>
  <SelectItem value="1">Type A</SelectItem>
  <SelectItem value="2">Type B</SelectItem>
</SelectExt>

// TreeSelect with info tooltip
<TreeSelectExt
  info="Select the parent category"
  categories={categories}
  value={field.value}
  onChange={field.onChange}
  placeholder="Select parent"
/>

// TreeMultiSelect with info tooltip
<TreeMultiSelectExt
  info="First selected category is the primary category"
  categories={categories}
  value={field.value}
  onChange={field.onChange}
  placeholder="Select categories"
/>

// Without info - renders base component
<InputExt placeholder="Enter name" {...field} />
```

**When to use `info` prop**:
- Field requires clarification or has special behavior
- Field has validation rules users should know about
- Replaces `FormDescription` when the hint is brief

**When NOT to use `info` prop**:
- Self-explanatory fields (e.g., "Product Name", "Description")
- Long explanations that need multi-line text (use `FormDescription` instead)

---

## Architecture

**Clean Architecture with 4 layers** - dependencies flow inward toward Domain:

```
API → Infrastructure → Application → Domain
```

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Domain** | `B2BCommerce.Backend.Domain` | Entities, Value Objects, Domain Services, Domain Events, Exceptions. Pure C#, NO external dependencies |
| **Application** | `B2BCommerce.Backend.Application` | DTOs, Service Interfaces, Repository Interfaces, CQRS Handlers, Validators, Domain Event Handlers. Depends only on Domain |
| **Infrastructure** | `B2BCommerce.Backend.Infrastructure` | EF Core DbContext, Repository implementations, Service implementations, Identity. Implements Application interfaces |
| **API** | `B2BCommerce.Backend.API` | Controllers, Middleware, Auth config. Thin layer, delegates via MediatR |
| **IntegrationAPI** | `B2BCommerce.Backend.IntegrationAPI` | External API for ERP integrations. API Key auth, delegates via MediatR |

---

## Domain Layer Rules

### Base Classes

**BaseEntity** - All entities inherit from this:
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    // Audit fields - PROTECTED setters (set by DbContext)
    public DateTime CreatedAt { get; protected set; }
    public string? CreatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    // Soft delete
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public string? DeletedBy { get; protected set; }

    // Domain events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public void SoftDelete(string? deletedBy = null) { ... }
    public void Restore() { ... }
}
```

**ExternalEntity** - For entities synced from external systems (ERP):
```csharp
public abstract class ExternalEntity : BaseEntity, IExternalEntity
{
    public string? ExternalId { get; protected set; }    // PRIMARY upsert key from external system
    public string? ExternalCode { get; protected set; }  // OPTIONAL additional reference code
    public DateTime? LastSyncedAt { get; protected set; }

    public void MarkAsSynced() => LastSyncedAt = DateTime.UtcNow;
    public void SetExternalIdentifiers(string? code, string? id) { ... }

    // Factory method pattern for external creation
    // Child entities implement: static T CreateFromExternal(string externalId, ...)
}
```

**Entities extending ExternalEntity**: `Product`, `Category`, `Brand`, `Customer`

**ExternalEntity Methods** (on child entities):
- `CreateFromExternal(externalId, name, ..., externalCode?, specificId?)` - Factory method for creating from external sync (externalId is PRIMARY key)
- `UpdateFromExternal(...)` - Method for updating from external sync
- `MarkAsSynced()` - Called after sync to set LastSyncedAt

### Entity Rules

1. **Private constructors** - Never use `new Entity()` directly
2. **Factory methods** - Use `Entity.Create(...)` or `Entity.CreateFromExternal(...)`
3. **Protected setters** - All properties have `{ get; protected set; }`
4. **Domain events** - Raise events in factory methods: `AddDomainEvent(new ProductCreatedEvent(this))`
5. **Behavior methods** - Encapsulate state changes: `order.AddItem(...)`, `customer.UpdateCreditLimit(...)`

```csharp
public class Product : ExternalEntity, IAggregateRoot
{
    // Private constructor
    private Product() { }

    // Factory method for internal creation
    public static Product Create(string sku, string name, Guid categoryId, ...)
    {
        var product = new Product { ... };
        product.AddDomainEvent(new ProductCreatedEvent(product));
        return product;
    }

    // Factory method for external system sync (ExternalId is PRIMARY key)
    public static Product CreateFromExternal(string externalId, string sku, string name, ..., string? externalCode = null, Guid? specificId = null)
    {
        var product = Create(sku, name, ...);
        if (specificId.HasValue) product.Id = specificId.Value;
        product.SetExternalIdentifiers(externalCode, externalId);
        product.MarkAsSynced();
        return product;
    }

    // Behavior methods
    public void UpdatePrice(Money newPrice) { ... }
    public void Deactivate() { ... }
}
```

### Marker Interfaces

- **IAggregateRoot** - Marks aggregate roots (only these should have repositories)
- **IExternalEntity** - Marks entities synced from external systems

### Value Objects

Immutable objects with value equality. Use for concepts with no identity:

```csharp
// Money - amount + currency with arithmetic
public record Money(decimal Amount, string Currency = "TRY")
{
    public static Money operator +(Money a, Money b) => ...
    public static Money operator *(Money m, decimal multiplier) => ...
}

// Email - validated wrapper
public record Email
{
    public string Value { get; }
    public Email(string value)
    {
        if (!IsValid(value)) throw new DomainException("Invalid email");
        Value = value;
    }
}

// Others: PhoneNumber, TaxNumber, Address
```

### Domain Events

Events raised when significant domain actions occur:

```csharp
// Interface
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}

// Events
public record ProductCreatedEvent(Product Product) : IDomainEvent { ... }
public record OrderPlacedEvent(Order Order) : IDomainEvent { ... }
public record CustomerCreditLimitChangedEvent(Customer Customer, decimal OldLimit, decimal NewLimit) : IDomainEvent { ... }
```

### Domain Exceptions

Never use `System.InvalidOperationException` in Domain layer:

```csharp
// Base exception
public class DomainException : Exception { ... }

// Specific exceptions
public class InvalidOperationDomainException : DomainException { ... }
public class InsufficientCreditException : DomainException { ... }
public class InsufficientStockException : DomainException { ... }
public class OutOfStockException : DomainException { ... }
```

### Domain Services

For logic spanning multiple aggregates:

```csharp
public interface IPricingService
{
    Money CalculatePrice(Product product, Customer customer, int quantity);
}

public interface ICreditManagementService
{
    bool HasSufficientCredit(Customer customer, Money amount);
    void ReserveCredit(Customer customer, Order order);
    void ReleaseCredit(Customer customer, Order order);
}
```

---

## Application Layer Rules

### Repository Interfaces

```csharp
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}

// Specific repositories for aggregate roots only
public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetBySKUAsync(string sku, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
}
```

### Unit of Work

```csharp
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ICustomerRepository Customers { get; }
    IOrderRepository Orders { get; }

    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
```

### DTOs

Naming convention: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`

```csharp
public record ProductDto(Guid Id, string Sku, string Name, decimal Price, ...);
public record CreateProductDto(string Sku, string Name, Guid CategoryId, ...);
public record UpdateProductDto(string Name, string? Description, decimal Price, ...);
```

### Result Pattern

Explicit success/failure handling:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public IEnumerable<string> Errors { get; }

    public static Result<T> Ok(T data) => new(true, data, null, null);
    public static Result<T> Fail(string error) => new(false, default, error, null);
    public static Result<T> Fail(IEnumerable<string> errors) => new(false, default, null, errors);
}
```

### CQRS with MediatR

**Commands** (write operations):
```csharp
public interface ICommand<TResponse> : IRequest<TResponse> { }
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse> { }

public record CreateProductCommand(string Sku, string Name, ...) : ICommand<Result<ProductDto>>;
```

**Queries** (read operations):
```csharp
public interface IQuery<TResponse> : IRequest<TResponse> { }
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> { }

public record GetProductByIdQuery(Guid Id) : IQuery<Result<ProductDto>>;
```

### FluentValidation

Every command MUST have a validator:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
            .MustAsync(async (sku, ct) => !await unitOfWork.Products.ExistsBySkuAsync(sku, ct))
            .WithMessage("SKU already exists");

        RuleFor(x => x.CategoryId)
            .MustAsync(async (id, ct) => await unitOfWork.Categories.ExistsAsync(id, ct))
            .WithMessage("Category not found");
    }
}
```

### Domain Event Handlers

Handle domain events in Application layer:

```csharp
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent notification, CancellationToken ct)
    {
        // Send notification, update cache, etc.
    }
}
```

### Application Exceptions

```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}

public class ValidationException : Exception
{
    public IEnumerable<string> Errors { get; }
}
```

---

## Infrastructure Layer Rules

### DbContext

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    // ...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Auto-set audit fields
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        // Dispatch domain events
        var domainEntities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();
        // ... dispatch and clear events

        return await base.SaveChangesAsync(ct);
    }
}
```

### Entity Configurations

Each entity has `IEntityTypeConfiguration<T>`:

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        // Global soft delete filter - REQUIRED
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Value objects
        builder.OwnsOne(p => p.ListPrice, money => {
            money.Property(m => m.Amount).HasColumnName("ListPriceAmount");
            money.Property(m => m.Currency).HasColumnName("ListPriceCurrency");
        });

        // Value object conversions
        builder.Property(p => p.Sku)
            .HasMaxLength(50)
            .IsRequired();
    }
}
```

### Repositories

```csharp
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    // Read operations use AsNoTracking()
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    // For update operations, get tracked entity
    public async Task<T?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
    }
}
```

### Services

Services implement Application interfaces:

```csharp
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<Result<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct)
    {
        // Use factory method - NEVER new Product()
        var product = Product.Create(dto.Sku, dto.Name, dto.CategoryId, ...);

        await _unitOfWork.Products.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<ProductDto>.Ok(_mapper.Map<ProductDto>(product));
    }
}
```

---

## API Layer Rules

### Controllers

Thin controllers - delegate to MediatR:

```csharp
[ApiController]
[Route("api/v1/[controller]")]  // ALWAYS use api/v1/ prefix
[Authorize]
public class ProductsController : BaseApiController
{
    private readonly IMediator _mediator;

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));

        if (!result.IsSuccess)
            return NotFoundResponse(result.ErrorMessage);

        return OkResponse(result.Data);
    }
}
```

### Middleware

**Required middleware** (in order):
1. `ExceptionHandlingMiddleware` - Global exception handling
2. `RequestLoggingMiddleware` - Request/response logging
3. Authentication/Authorization

```csharp
// Program.cs
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
```

### Authentication

**Main API**: JWT Bearer tokens
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });
```

**Integration API**: API Key authentication
```csharp
services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", null);
```

### Integration API Endpoints

For ExternalEntity entities, provide ExternalId-based upsert and lookup endpoints:

**External Entity Identity Fields**:
- `ExternalId` (string) - **PRIMARY upsert key** from external system (e.g., LOGO ERP ID)
- `ExternalCode` (string) - **OPTIONAL** additional reference code
- `Id` (Guid) - Internal database ID

**Upsert Endpoint** - Creates or updates by ExternalId or Id:
```csharp
// POST /api/v1/categories - Upsert by ExtId or Id
[HttpPost]
[Authorize(Policy = "categories:write")]
public async Task<IActionResult> UpsertCategory([FromBody] CategorySyncRequest request)
{
    // One of ExtId or Id is required
    // If only Id provided, ExtId is set to Id.ToString()
    var command = new UpsertCategoryCommand
    {
        Id = request.Id,                      // Optional internal ID (for update or create with specific ID)
        ExternalId = request.ExtId,           // PRIMARY upsert key
        ExternalCode = request.ExtCode,       // Optional reference
        Name = request.Name,
        ParentExternalId = request.ParentId,  // Parent lookup by ExternalId
        ParentExternalCode = request.ParentCode,  // Parent lookup by ExternalCode (fallback)
        ...
    };
    return OkResponse(await _mediator.Send(command));
}
```

**Lookup Endpoints**:
```csharp
// GET /api/v1/categories/{id:guid} - By internal ID
// GET /api/v1/categories/ext/{extId} - By ExternalId (primary lookup)
// GET /api/v1/categories/ext/{extId}/subcategories - Get subcategories by parent's ExternalId
```

**Action Endpoints by ExternalId**:
```csharp
// DELETE /api/v1/categories/ext/{extId} - Delete by ExternalId
// POST /api/v1/categories/ext/{extId}/activate - Activate by ExternalId
// POST /api/v1/categories/ext/{extId}/deactivate - Deactivate by ExternalId
```

**Repository Methods for ExternalEntity**:
```csharp
public interface ICategoryRepository : IGenericRepository<Category>
{
    // PRIMARY lookup by ExternalId
    Task<Category?> GetByExternalIdAsync(string externalId, CancellationToken ct = default);
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken ct = default);

    // FALLBACK lookup by ExternalCode (backward compatibility)
    Task<Category?> GetByExternalCodeAsync(string externalCode, CancellationToken ct = default);
    Task<bool> ExistsByExternalCodeAsync(string externalCode, CancellationToken ct = default);
}
```

### Integration API - AttributeDefinitions

AttributeDefinition extends ExternalEntity for ERP synchronization. Endpoints follow the same pattern as Categories.

**Endpoints**:
```csharp
// List & Get
GET /api/v1/attributes                              // List (with ?includeValues=true)
GET /api/v1/attributes/{id:guid}                    // By internal ID
GET /api/v1/attributes/ext/{extId}                  // By ExternalId
GET /api/v1/attributes/{id}/values/{valueId}        // Get specific value
GET /api/v1/attributes/ext/{extId}/values/{valueId} // Get value by attribute extId

// Upsert
POST /api/v1/attributes                             // Upsert attribute (with values)
POST /api/v1/attributes/{id}/values                 // Upsert value by attribute ID
POST /api/v1/attributes/ext/{extId}/values          // Upsert value by attribute extId

// Delete
DELETE /api/v1/attributes/{id}                      // Delete attribute
DELETE /api/v1/attributes/ext/{extId}               // Delete by ExternalId
DELETE /api/v1/attributes/{id}/values/{valueId}     // Delete value
DELETE /api/v1/attributes/ext/{extId}/values/{valueId}
```

**Upsert AttributeDefinition Request**:
```json
{
  "id": "guid (optional)",
  "extId": "string (primary upsert key)",
  "extCode": "string (optional)",
  "code": "string (required, unique)",
  "name": "string (required)",
  "nameEn": "string (optional)",
  "type": "Text|Number|Select|MultiSelect|Boolean|Date",
  "unit": "string (optional)",
  "isFilterable": true,
  "isRequired": false,
  "isVisibleOnProductPage": true,
  "displayOrder": 0,
  "values": [
    { "value": "string", "displayText": "string", "displayOrder": 0 }
  ]
}
```

**Notes**:
- One of `extId` or `id` is required for upsert
- If only `id` provided, `extId` is set to `id.ToString()`
- `values` use full replacement semantics (provided values replace all existing)
- Authorization: `attributes:read`, `attributes:write` policies

### Response Format

Consistent response wrapper:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}
```

---

## Coding Standards

### Null Handling

```csharp
// ✅ GOOD - Pattern matching
if (product is null) return NotFound();
if (customer is not null) DoSomething();

// ❌ BAD - Equality operators
if (product == null) return NotFound();
if (customer != null) DoSomething();

// ⚠️ EXCEPTION - Expression trees (AutoMapper, EF LINQ)
.ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name != null ? s.Name : ""))
.Where(p => p.Category != null && p.Category.IsActive)
```

### Async/Await

```csharp
// ✅ All async methods suffixed with Async
public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)

// ✅ Always use async/await
return await _repository.GetByIdAsync(id, ct);

// ❌ NEVER use .Result or .Wait()
return _repository.GetByIdAsync(id).Result;  // Deadlock risk!
```

### Exception Handling

```csharp
// ✅ GOOD - Specific exceptions
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(ex, "Concurrency conflict for {EntityId}", id);
    throw new ConflictException("Record was modified");
}

// ❌ BAD - Generic exception
catch (Exception ex)
{
    _logger.LogError(ex, "Error");
    throw;
}
```

### Logging

```csharp
// ✅ Structured logging
_logger.LogInformation("Product created: {ProductId} - {Sku}", product.Id, product.Sku);

// ❌ String interpolation
_logger.LogInformation($"Product created: {product.Id} - {product.Sku}");
```

### Braces

```csharp
// ✅ GOOD - Always use braces
if (product is null)
{
    return NotFound();
}

// ❌ BAD - No braces
if (product is null)
    return NotFound();
```

---

## Project Structure

```
backend/src/
├── B2BCommerce.Backend.Domain/
│   ├── Common/              # BaseEntity, ExternalEntity, IAggregateRoot
│   ├── Entities/            # Product, Category, Order, etc.
│   ├── ValueObjects/        # Money, Address, Email, etc.
│   ├── Enums/               # OrderStatus, PaymentStatus, etc.
│   ├── Events/              # IDomainEvent, ProductCreatedEvent, etc.
│   ├── DomainServices/      # IPricingService, ICreditManagementService
│   └── Exceptions/          # DomainException, InvalidOperationDomainException
│
├── B2BCommerce.Backend.Application/
│   ├── Common/              # Result<T>, PagedResult<T>, ICommand, IQuery
│   ├── DTOs/                # CategoryDto, ProductDto, etc.
│   ├── Exceptions/          # NotFoundException, ValidationException
│   ├── Features/            # CQRS Commands & Queries by feature
│   │   └── Categories/
│   │       ├── Commands/    # CreateCategoryCommand, UpdateCategoryCommand
│   │       ├── Queries/     # GetCategoriesQuery, GetCategoryByIdQuery
│   │       └── Events/      # CategoryCreatedEventHandler
│   ├── Interfaces/
│   │   ├── Repositories/    # IGenericRepository, IProductRepository
│   │   └── Services/        # ICategoryService, IProductService
│   ├── Mappings/            # AutoMapper profiles
│   └── Validators/          # FluentValidation validators
│
├── B2BCommerce.Backend.Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/  # EF Core entity configurations
│   │   ├── Repositories/    # Repository implementations
│   │   └── Migrations/
│   ├── Services/            # Service implementations
│   └── Identity/            # ApplicationUser, ApplicationRole
│
├── B2BCommerce.Backend.API/
│   ├── Controllers/         # MediatR-based controllers
│   ├── Middleware/          # ExceptionHandling, RequestLogging
│   └── Program.cs
│
└── B2BCommerce.Backend.IntegrationAPI/
    ├── Controllers/         # MediatR-based controllers
    ├── Authentication/      # ApiKeyAuthenticationHandler
    └── Program.cs
```

---

## Quick Reference

| Rule | Do | Don't |
|------|-----|-------|
| Entity creation | `Product.Create(...)` | `new Product(...)` |
| Null checks | `is null` / `is not null` | `== null` / `!= null` |
| Domain exceptions | `throw new InvalidOperationDomainException(...)` | `throw new InvalidOperationException(...)` |
| Controllers | Delegate to MediatR | Access DbContext directly |
| Async methods | Suffix with `Async` | Omit suffix |
| Repositories | Only for aggregate roots | For every entity |
| Soft delete | `HasQueryFilter(e => !e.IsDeleted)` | Manual filtering |
| Read queries | `AsNoTracking()` | Track read-only entities |

---

## Documentation

Detailed specifications in `backend/docs/`:
- `00-README.md` - Documentation index
- `01-Solution-Structure.md` - Project organization
- `02-Architecture-Guide.md` - Clean Architecture principles
- `03-Domain-Layer-Guide.md` - Entities, value objects, events
- `04-Application-Layer-Guide.md` - DTOs, CQRS, validation
- `05-Infrastructure-Layer-Guide.md` - DbContext, repositories
- `06-API-Layer-Guide.md` - Controllers, authentication
- `07-Coding-Standards.md` - Naming, formatting, best practices
- `08-Testing-Guide.md` - Test structure and patterns
