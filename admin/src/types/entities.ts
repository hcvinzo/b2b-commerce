// Base entity with common fields
export interface BaseEntity {
  id: string;
  createdAt: string;
  createdBy?: string;
  updatedAt?: string;
  updatedBy?: string;
  isDeleted: boolean;
}

// External entity (synced from ERP)
export interface ExternalEntity extends BaseEntity {
  externalId?: string;
  externalCode?: string;
  lastSyncedAt?: string;
}

// Category
export interface Category extends ExternalEntity {
  name: string;
  slug: string;
  description?: string;
  parentId?: string;
  parentName?: string;
  imageUrl?: string;
  displayOrder: number;
  isActive: boolean;
  level: number;
  path: string;
  productCount: number;
  children?: Category[];
}

export interface CreateCategoryDto {
  name: string;
  description?: string;
  parentCategoryId?: string;
  imageUrl?: string;
  displayOrder?: number;
  isActive?: boolean;
}

export interface UpdateCategoryDto extends Partial<CreateCategoryDto> {}

// Product Status
export type ProductStatus = "Draft" | "Active" | "Inactive";

// Numeric values for ProductStatus (matching backend)
export const ProductStatusValues = {
  Draft: 0,
  Active: 1,
  Inactive: 2,
} as const;

// Product Category (junction table)
export interface ProductCategory {
  id: string;
  categoryId: string;
  categoryName: string;
  categorySlug: string;
  isPrimary: boolean;
  displayOrder: number;
}

// Product
export interface Product extends ExternalEntity {
  sku: string;
  name: string;
  slug: string;
  description?: string;
  /** Primary category ID (for backward compatibility) */
  categoryId: string;
  /** Primary category name (for backward compatibility) */
  categoryName?: string;
  /** All categories this product belongs to */
  categories: ProductCategory[];
  brandId?: string;
  brandName?: string;
  productTypeId?: string;
  productTypeName?: string;
  listPrice: number;
  listPriceCurrency: string;
  dealerPrice?: number;
  dealerPriceCurrency?: string;
  tier1Price?: number;
  tier2Price?: number;
  tier3Price?: number;
  tier4Price?: number;
  tier5Price?: number;
  stockQuantity: number;
  minOrderQuantity: number;
  maxOrderQuantity?: number;
  unitOfMeasure: string;
  weight?: number;
  length?: number;
  width?: number;
  height?: number;
  /** Product lifecycle status (Draft, Active, Inactive) */
  status: ProductStatus;
  /** @deprecated Use status === "Active" instead */
  isActive: boolean;
  isFeatured: boolean;
  isSerialTracked: boolean;
  taxRate: number;
  mainImageUrl?: string;
  imageUrls?: string[];
  images: ProductImage[];
  specifications: ProductSpecification[];
  attributes: ProductAttribute[];
  attributeValues: ProductAttributeValueOutput[];
}

export interface ProductImage {
  id: string;
  url: string;
  altText?: string;
  displayOrder: number;
  isPrimary: boolean;
}

export interface ProductSpecification {
  key: string;
  value: string;
  displayOrder: number;
}

export interface ProductAttribute {
  attributeId: string;
  attributeName: string;
  value: string;
}

// Product Attribute Value (from backend ProductAttributeValueOutputDto)
export interface ProductAttributeValueOutput {
  attributeDefinitionId: string;
  attributeCode: string;
  attributeName: string;
  attributeType: AttributeType;
  unit?: string;
  textValue?: string;
  numericValue?: number;
  selectValueId?: string;
  selectValueText?: string;
  multiSelectValueIds?: string[];
  multiSelectValueTexts?: string[];
  booleanValue?: boolean;
  dateValue?: string;
}

// Product Attribute Value Input (for create/update operations)
export interface ProductAttributeValueInput {
  attributeDefinitionId: string;
  textValue?: string;
  numericValue?: number;
  selectValueId?: string;
  multiSelectValueIds?: string[];
  booleanValue?: boolean;
  dateValue?: string;
}

export interface CreateProductDto {
  sku: string;
  name: string;
  description?: string;
  /** Category IDs (first one becomes primary) */
  categoryIds: string[];
  brandId?: string;
  productTypeId?: string;
  listPrice: number;
  currency: string;
  dealerPrice?: number;
  tier1Price?: number;
  tier2Price?: number;
  tier3Price?: number;
  tier4Price?: number;
  tier5Price?: number;
  stockQuantity?: number;
  minOrderQuantity?: number;
  maxOrderQuantity?: number;
  unitOfMeasure?: string;
  isSerialTracked?: boolean;
  taxRate?: number;
  mainImageUrl?: string;
  imageUrls?: string[];
  weight?: number;
  length?: number;
  width?: number;
  height?: number;
  /** Product status (Draft, Active, Inactive) */
  status?: ProductStatus;
  /** @deprecated Use status instead */
  isActive?: boolean;
  isFeatured?: boolean;
  attributeValues?: ProductAttributeValueInput[];
}

export interface UpdateProductDto extends Partial<CreateProductDto> {}

export interface ProductFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: string;
  brandId?: string;
  /** Filter by status */
  status?: ProductStatus;
  /** @deprecated Use status instead */
  isActive?: boolean;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
}

// Customer
export interface Customer extends ExternalEntity {
  companyName: string;
  tradeName?: string;
  taxNumber: string;
  taxOffice?: string;
  email: string;
  phone?: string;
  website?: string;
  creditLimit: number;
  creditLimitCurrency: string;
  usedCredit: number;
  availableCredit: number;
  paymentTermDays: number;
  discountRate: number;
  priceTier: number;
  isActive: boolean;
  isApproved: boolean;
  approvedAt?: string;
  approvedBy?: string;
  primaryAddress?: Address;
  addresses: Address[];
  contacts: CustomerContact[];
}

export interface Address {
  id: string;
  title: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  district?: string;
  postalCode?: string;
  country: string;
  isDefault: boolean;
  isBilling: boolean;
  isShipping: boolean;
}

export interface CustomerContact {
  id: string;
  name: string;
  title?: string;
  email?: string;
  phone?: string;
  isPrimary: boolean;
}

export interface CustomerFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
  isApproved?: boolean;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
}

// Order
export interface Order extends BaseEntity {
  orderNumber: string;
  customerId: string;
  customerName: string;
  status: OrderStatus;
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  shippingAmount: number;
  total: number;
  currency: string;
  notes?: string;
  internalNotes?: string;
  shippingAddressId?: string;
  billingAddressId?: string;
  shippingAddress?: Address;
  billingAddress?: Address;
  items: OrderItem[];
  statusHistory: OrderStatusHistory[];
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  discountAmount: number;
  taxRate: number;
  taxAmount: number;
  lineTotal: number;
}

export interface OrderStatusHistory {
  id: string;
  status: OrderStatus;
  notes?: string;
  createdAt: string;
  createdBy?: string;
}

export type OrderStatus =
  | "Pending"
  | "Confirmed"
  | "Processing"
  | "Shipped"
  | "Delivered"
  | "Cancelled"
  | "Returned";

export interface OrderFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: OrderStatus;
  customerId?: string;
  fromDate?: string;
  toDate?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
}

// Brand
export interface Brand extends ExternalEntity {
  name: string;
  nameEn?: string;
  slug: string;
  description?: string;
  logoUrl?: string;
  websiteUrl?: string;
  isActive: boolean;
  displayOrder: number;
  productCount: number;
}

// Product Type
export interface ProductType extends BaseEntity {
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
  attributes: ProductTypeAttribute[];
  externalId?: string;
  externalCode?: string;
  lastSyncedAt?: string;
}

// Product Type for list views (lighter)
export interface ProductTypeListItem extends BaseEntity {
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
  attributeCount: number;
  productCount: number;
  externalId?: string;
  externalCode?: string;
  lastSyncedAt?: string;
}

// Product Type Attribute (junction between ProductType and AttributeDefinition)
export interface ProductTypeAttribute {
  id: string;
  attributeDefinitionId: string;
  attributeExternalId?: string;
  attributeCode: string;
  attributeName: string;
  attributeType: AttributeType;
  unit?: string;
  isRequired: boolean; // Overrides AttributeDefinition.isRequired for this product type
  displayOrder: number;
  predefinedValues?: AttributeValue[];
}

// DTOs for ProductType operations
export interface CreateProductTypeDto {
  code: string;
  name: string;
  description?: string;
  attributes?: AddAttributeToProductTypeDto[];
}

export interface UpdateProductTypeDto {
  name: string;
  description?: string;
  isActive?: boolean;
}

export interface AddAttributeToProductTypeDto {
  attributeDefinitionId: string;
  isRequired?: boolean;
  displayOrder?: number;
}

// Attribute Definition
export interface AttributeDefinition extends ExternalEntity {
  code: string;
  name: string;
  nameEn?: string;
  type: AttributeType;
  unit?: string;
  isFilterable: boolean;
  isRequired: boolean;
  isVisibleOnProductPage: boolean;
  displayOrder: number;
  predefinedValues: AttributeValue[];
}

export interface AttributeValue {
  id: string;
  value: string;
  displayText?: string;
  displayOrder: number;
}

// Backend sends integer enum values: 1=Text, 2=Number, 3=Select, 4=MultiSelect, 5=Boolean, 6=Date
export type AttributeType = 1 | 2 | 3 | 4 | 5 | 6;

export const AttributeTypeEnum = {
  Text: 1,
  Number: 2,
  Select: 3,
  MultiSelect: 4,
  Boolean: 5,
  Date: 6,
} as const;

export interface CreateAttributeDefinitionDto {
  code: string;
  name: string;
  type: AttributeType | string;
  unit?: string;
  isFilterable?: boolean;
  isRequired?: boolean;
  isVisibleOnProductPage?: boolean;
  displayOrder?: number;
  predefinedValues?: CreateAttributeValueDto[];
}

export interface UpdateAttributeDefinitionDto {
  name?: string;
  unit?: string;
  isFilterable?: boolean;
  isRequired?: boolean;
  isVisibleOnProductPage?: boolean;
  displayOrder?: number;
}

export interface CreateAttributeValueDto {
  value: string;
  displayText?: string;
  displayOrder?: number;
}

// User
export interface User {
  id: string;
  email: string;
  name: string;
  role: string;
  avatarUrl?: string;
  isActive: boolean;
  lastLoginAt?: string;
}

// Dashboard Stats
export interface DashboardStats {
  totalRevenue: number;
  revenueChange: number;
  totalOrders: number;
  ordersChange: number;
  totalCustomers: number;
  customersChange: number;
  activeProducts: number;
  productsChange: number;
}

export interface SalesDataPoint {
  month: string;
  sales: number;
  orders: number;
}

export interface TopProduct {
  id: string;
  name: string;
  sku: string;
  totalSales: number;
  quantitySold: number;
}

// API Client (for list views)
export interface ApiClientListItem {
  id: string;
  name: string;
  contactEmail: string;
  isActive: boolean;
  activeKeyCount: number;
  createdAt: string;
}

// API Client (detailed view with keys)
export interface ApiClient {
  id: string;
  name: string;
  description?: string;
  contactEmail: string;
  contactPhone?: string;
  isActive: boolean;
  createdAt: string;
  createdBy?: string;
  updatedAt?: string;
  updatedBy?: string;
  apiKeys: ApiKeyListItem[];
}

// API Key (for list views)
export interface ApiKeyListItem {
  id: string;
  keyPrefix: string;
  name: string;
  isActive: boolean;
  isExpired: boolean;
  isRevoked: boolean;
  expiresAt?: string;
  lastUsedAt?: string;
  rateLimitPerMinute: number;
  permissionCount: number;
  createdAt: string;
}

// API Key (detailed view)
export interface ApiKeyDetail {
  id: string;
  apiClientId: string;
  apiClientName: string;
  keyPrefix: string;
  name: string;
  isActive: boolean;
  isExpired: boolean;
  isRevoked: boolean;
  expiresAt?: string;
  lastUsedAt?: string;
  lastUsedIp?: string;
  rateLimitPerMinute: number;
  revokedAt?: string;
  revokedBy?: string;
  revocationReason?: string;
  createdAt: string;
  createdBy?: string;
  permissions: string[];
  ipWhitelist: IpWhitelistEntry[];
}

export interface IpWhitelistEntry {
  id: string;
  ipAddress: string;
  description?: string;
}

// Create API Key Response (includes plaintext key - shown once)
export interface CreateApiKeyResponse {
  id: string;
  keyPrefix: string;
  plainTextKey: string;
  name: string;
  expiresAt?: string;
  permissions: string[];
  warning: string;
}

// DTOs for API Client operations
export interface CreateApiClientDto {
  name: string;
  description?: string;
  contactEmail: string;
  contactPhone?: string;
}

export interface UpdateApiClientDto {
  name: string;
  description?: string;
  contactEmail: string;
  contactPhone?: string;
}

// DTOs for API Key operations
export interface CreateApiKeyDto {
  apiClientId: string;
  name: string;
  rateLimitPerMinute?: number;
  expiresAt?: string;
  permissions?: string[];
}

export interface RevokeApiKeyDto {
  reason: string;
}

export interface AddIpWhitelistDto {
  ipAddress: string;
  description?: string;
}

// Filters
export interface ApiClientFilters {
  page?: number;
  pageSize?: number;
  isActive?: boolean;
}

// Available permission scopes
export interface PermissionScope {
  name: string;
  scopes: string[];
}

export interface AvailableScopesResponse {
  scopes: string[];
  categories: PermissionScope[];
}
