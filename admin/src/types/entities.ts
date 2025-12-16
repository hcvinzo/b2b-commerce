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

// Product
export interface Product extends ExternalEntity {
  sku: string;
  name: string;
  nameEn?: string;
  slug: string;
  description?: string;
  descriptionEn?: string;
  categoryId: string;
  categoryName?: string;
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
  width?: number;
  height?: number;
  depth?: number;
  isActive: boolean;
  isFeatured: boolean;
  isSerialTracked: boolean;
  images: ProductImage[];
  specifications: ProductSpecification[];
  attributes: ProductAttribute[];
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

export interface CreateProductDto {
  sku: string;
  name: string;
  nameEn?: string;
  description?: string;
  categoryId: string;
  brandId?: string;
  productTypeId?: string;
  listPrice: number;
  listPriceCurrency?: string;
  dealerPrice?: number;
  stockQuantity?: number;
  minOrderQuantity?: number;
  maxOrderQuantity?: number;
  unitOfMeasure?: string;
  isActive?: boolean;
  isFeatured?: boolean;
}

export interface UpdateProductDto extends Partial<CreateProductDto> {}

export interface ProductFilters {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: string;
  brandId?: string;
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
  nameEn?: string;
  description?: string;
  isActive: boolean;
  attributeDefinitions: AttributeDefinition[];
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
  values: AttributeValue[];
}

export interface AttributeValue {
  id: string;
  value: string;
  displayText?: string;
  displayOrder: number;
}

export type AttributeType =
  | "Text"
  | "Number"
  | "Select"
  | "MultiSelect"
  | "Boolean"
  | "Date";

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
