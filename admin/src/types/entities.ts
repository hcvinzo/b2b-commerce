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

// Customer Status
export type CustomerStatus = "Pending" | "Active" | "Suspended" | "Rejected";

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

// Customer Address Type
export type CustomerAddressType = "Billing" | "Shipping" | "Contact";

// Customer Address
export interface CustomerAddress {
  id: string;
  customerId: string;
  title: string;
  fullName?: string;
  addressType: CustomerAddressType;
  address: string;
  geoLocationId?: string;
  geoLocationPathName?: string;
  postalCode?: string;
  phone?: string;
  phoneExt?: string;
  gsm?: string;
  taxNo?: string;
  isDefault: boolean;
  isActive: boolean;
}

// Customer
export interface Customer extends BaseEntity {
  // Company Information
  title: string;
  taxOffice?: string;
  taxNo?: string;
  establishmentYear?: number;
  website?: string;

  // Status
  status: CustomerStatus;

  // Document URLs (JSON)
  documentUrls?: string;

  // Relations
  contacts: CustomerContact[];
  addresses: CustomerAddress[];
  attributes: CustomerAttribute[];
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
  customerId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email?: string;
  position?: string;
  dateOfBirth?: string;
  gender: string;
  phone?: string;
  phoneExt?: string;
  gsm?: string;
  isPrimary: boolean;
  isActive: boolean;
}

export interface CustomerFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: CustomerStatus;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
}

export interface UpdateCustomerData {
  title: string;
  taxOffice?: string;
  taxNo?: string;
  establishmentYear?: number;
  website?: string;
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
// Note: Backend uses JsonStringEnumConverter, so type and entityType come as strings
export interface AttributeDefinition extends ExternalEntity {
  code: string;
  name: string;
  nameEn?: string;
  type: AttributeType | AttributeTypeString;
  entityType?: AttributeEntityType | AttributeEntityTypeString;
  parentAttributeId?: string;
  isList?: boolean;
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

// Backend sends enum as strings due to JsonStringEnumConverter
export type AttributeTypeString = "Text" | "Number" | "Select" | "MultiSelect" | "Boolean" | "Date" | "Composite";
export type AttributeType = 1 | 2 | 3 | 4 | 5 | 6 | 7;

export const AttributeTypeEnum = {
  Text: 1,
  Number: 2,
  Select: 3,
  MultiSelect: 4,
  Boolean: 5,
  Date: 6,
  Composite: 7,
} as const;

// Map string type to integer for consistent handling
export const AttributeTypeFromString: Record<AttributeTypeString, AttributeType> = {
  Text: 1,
  Number: 2,
  Select: 3,
  MultiSelect: 4,
  Boolean: 5,
  Date: 6,
  Composite: 7,
};

// Backend sends enum as strings due to JsonStringEnumConverter
export type AttributeEntityTypeString = "Product" | "Customer";
export type AttributeEntityType = 1 | 2;

export const AttributeEntityTypeEnum = {
  Product: 1,
  Customer: 2,
} as const;

// Map string entityType to integer for consistent handling
export const AttributeEntityTypeFromString: Record<AttributeEntityTypeString, AttributeEntityType> = {
  Product: 1,
  Customer: 2,
};

export const AttributeEntityTypeLabels: Record<AttributeEntityType, string> = {
  1: "Product",
  2: "Customer",
};

export interface CreateAttributeDefinitionDto {
  code: string;
  name: string;
  type: AttributeType | string;
  entityType?: AttributeEntityType;
  parentAttributeId?: string;
  isList?: boolean;
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

// =====================
// Admin Users
// =====================

// Admin User (for list views)
export interface AdminUserListItem {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  fullName: string;
  roles: string[];
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
}

// Admin User (detailed view)
export interface AdminUser {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  roles: string[];
  isActive: boolean;
  emailConfirmed: boolean;
  lastLoginAt?: string;
  createdAt: string;
}

// DTOs for Admin User operations
export interface CreateAdminUserDto {
  email: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  roles: string[];
  temporaryPassword?: string;
  sendWelcomeEmail?: boolean;
}

export interface UpdateAdminUserDto {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  roles?: string[];
}

// Filters
export interface AdminUserFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

// Available roles
export interface AvailableRole {
  name: string;
  description?: string;
}

// User login (external provider)
export interface UserLogin {
  id: string;
  loginProvider: string;
  providerDisplayName: string;
  providerKey?: string;
}

// User claim
export interface UserClaim {
  id: number;
  type: string;
  value: string;
}

// DTOs for user claim operations
export interface AddUserClaimDto {
  type: string;
  value: string;
}

// DTO for setting user roles
export interface SetUserRolesDto {
  roles: string[];
}

// ============================================
// Role Management Types
// ============================================

// Role (list view)
export interface RoleListItem {
  id: string;
  name: string;
  description?: string;
  userCount: number;
  claimCount: number;
  isProtected: boolean;
  isSystemRole: boolean;
  createdAt: string;
}

// Role (detailed view)
export interface RoleDetail {
  id: string;
  name: string;
  description?: string;
  claims: string[];
  userCount: number;
  isProtected: boolean;
  isSystemRole: boolean;
  createdAt: string;
}

// DTOs for Role operations
export interface CreateRoleDto {
  name: string;
  description?: string;
  claims?: string[];
}

export interface UpdateRoleDto {
  name?: string;
  description?: string;
}

export interface SetRoleClaimsDto {
  claims: string[];
}

// Role user list item
export interface RoleUserListItem {
  id: string;
  email: string;
  fullName?: string;
  isActive: boolean;
}

// Available permission
export interface AvailablePermission {
  value: string;
  displayName: string;
  description?: string;
  category: string;
}

// Permission category
export interface PermissionCategory {
  name: string;
  description?: string;
  permissions: AvailablePermission[];
}

// Filters
export interface RoleFilters {
  page?: number;
  pageSize?: number;
  search?: string;
}

// ============================================
// Customer Attribute Types
// ============================================

// Customer Attribute (based on AttributeDefinition)
export interface CustomerAttribute {
  id: string;
  customerId: string;
  attributeDefinitionId: string;
  attributeCode: string;
  attributeName: string;
  value: string; // JSON string containing attribute data
  createdAt: string;
  updatedAt?: string;
}

// DTO for upserting attributes by definition
export interface UpsertCustomerAttributesDto {
  attributeDefinitionId: string;
  items: CustomerAttributeItemDto[];
}

export interface CustomerAttributeItemDto {
  id?: string;
  value: string; // JSON string containing attribute data
}

// ============================================
// Product Relation Types
// ============================================

export type ProductRelationType = "Related" | "CrossSell" | "UpSell" | "Accessories";

export const ProductRelationTypeEnum: ProductRelationType[] = [
  "Related",
  "CrossSell",
  "UpSell",
  "Accessories",
];

export const ProductRelationTypeLabels: Record<ProductRelationType, string> = {
  Related: "Related Products",
  CrossSell: "Cross-sell",
  UpSell: "Up-sell",
  Accessories: "Accessories",
};

export interface ProductRelation {
  id: string;
  relatedProductId: string;
  relatedProductName: string;
  relatedProductSku: string;
  relatedProductImageUrl?: string;
  relatedProductPrice: number;
  relatedProductIsActive: boolean;
  relationType: ProductRelationType;
  displayOrder: number;
}

export interface ProductRelationsGroup {
  relationType: ProductRelationType;
  relationTypeName: string;
  relations: ProductRelation[];
}

export interface RelatedProductInput {
  productId: string;
  displayOrder: number;
}

// Lightweight product for selection dropdowns
export interface ProductListItem {
  id: string;
  sku: string;
  name: string;
  mainImageUrl?: string;
  listPrice: number;
  isActive: boolean;
}

// ============================================
// Collection Types (Product Groupings)
// ============================================

// Collection Type enum (matching backend)
export type CollectionType = "Manual" | "Dynamic";

export const CollectionTypeEnum = {
  Manual: 1,
  Dynamic: 2,
} as const;

export const CollectionTypeLabels: Record<CollectionType, string> = {
  Manual: "Manual",
  Dynamic: "Dynamic",
};

// Collection filter (for dynamic collections)
export interface CollectionFilter {
  categoryIds?: string[];
  brandIds?: string[];
  productTypeIds?: string[];
  minPrice?: number;
  maxPrice?: number;
}

// Collection (detailed view)
export interface Collection extends ExternalEntity {
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  type: CollectionType;
  displayOrder: number;
  isActive: boolean;
  isFeatured: boolean;
  startDate?: string;
  endDate?: string;
  isCurrentlyActive: boolean;
  productCount: number;
  filter?: CollectionFilter;
}

// Collection (list view)
export interface CollectionListItem {
  id: string;
  name: string;
  slug: string;
  imageUrl?: string;
  type: CollectionType;
  isActive: boolean;
  isFeatured: boolean;
  productCount: number;
  startDate?: string;
  endDate?: string;
  isCurrentlyActive: boolean;
  createdAt: string;
}

// Product in collection (for manual collections)
export interface ProductInCollection {
  productId: string;
  productName: string;
  productSku: string;
  productImageUrl?: string;
  productPrice: number;
  productIsActive: boolean;
  displayOrder: number;
  isFeatured: boolean;
}

// DTOs for Collection operations
export interface CreateCollectionDto {
  name: string;
  description?: string;
  imageUrl?: string;
  type: CollectionType;
  displayOrder?: number;
  isActive?: boolean;
  isFeatured?: boolean;
  startDate?: string;
  endDate?: string;
}

export interface UpdateCollectionDto {
  name: string;
  description?: string;
  imageUrl?: string;
  displayOrder?: number;
  isActive?: boolean;
  isFeatured?: boolean;
  startDate?: string;
  endDate?: string;
}

// Set products for manual collection
export interface SetCollectionProductsDto {
  products: ProductInCollectionInput[];
}

export interface ProductInCollectionInput {
  productId: string;
  displayOrder: number;
  isFeatured: boolean;
}

// Set filters for dynamic collection
export interface SetCollectionFiltersDto {
  categoryIds?: string[];
  brandIds?: string[];
  productTypeIds?: string[];
  minPrice?: number;
  maxPrice?: number;
}

// Filters for collection list
export interface CollectionFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  type?: CollectionType;
  isActive?: boolean;
  isFeatured?: boolean;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}

// ============================================
// GeoLocationType Types
// ============================================

export interface GeoLocationType {
  id: string;
  name: string;
  displayOrder: number;
  locationCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateGeoLocationTypeDto {
  name: string;
  displayOrder: number;
}

export interface UpdateGeoLocationTypeDto {
  name: string;
  displayOrder: number;
}

export interface GeoLocationTypeFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}

// ============================================
// GeoLocation Types
// ============================================

export interface GeoLocation {
  id: string;
  code: string;
  name: string;
  geoLocationTypeId: string;
  geoLocationType?: GeoLocationType;
  parentId?: string;
  parent?: GeoLocation;
  latitude?: number;
  longitude?: number;
  metadata?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface GeoLocationListItem {
  id: string;
  code: string;
  name: string;
  geoLocationTypeId: string;
  geoLocationTypeName: string;
  parentId?: string;
  parentName?: string;
  latitude?: number;
  longitude?: number;
  isActive: boolean;
}

export interface GeoLocationTree {
  id: string;
  code: string;
  name: string;
  geoLocationTypeId: string;
  geoLocationTypeName: string;
  parentId?: string;
  latitude?: number;
  longitude?: number;
  isActive: boolean;
  children?: GeoLocationTree[];
}

export interface CreateGeoLocationDto {
  geoLocationTypeId: string;
  code: string;
  name: string;
  parentId?: string;
  latitude?: number;
  longitude?: number;
  metadata?: string;
}

export interface UpdateGeoLocationDto {
  code: string;
  name: string;
  latitude?: number;
  longitude?: number;
  metadata?: string;
}

export interface GeoLocationFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  typeId?: string;
  parentId?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}

// ============================================
// Customer User Types (Dealer Multi-User)
// ============================================

// Customer user role
export interface CustomerUserRole {
  id: string;
  name: string;
  description?: string;
}

// Customer user (list view)
export interface CustomerUserListItem {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  fullName: string;
  roles: CustomerUserRole[];
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
}

// Customer user (detailed view)
export interface CustomerUser {
  id: string;
  customerId: string;
  customerTitle: string;
  email: string;
  firstName?: string;
  lastName?: string;
  fullName: string;
  phoneNumber?: string;
  roles: CustomerUserRole[];
  isActive: boolean;
  emailConfirmed: boolean;
  lastLoginAt?: string;
  createdAt: string;
}

// DTOs for Customer User operations
export interface CreateCustomerUserDto {
  email: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  roleIds: string[];
  temporaryPassword?: string;
  sendWelcomeEmail?: boolean;
}

export interface UpdateCustomerUserDto {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
}

export interface SetCustomerUserRolesDto {
  roleIds: string[];
}

// Filters for customer user list
export interface CustomerUserFilters {
  page?: number;
  pageSize?: number;
  search?: string;
}
