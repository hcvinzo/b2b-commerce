import {
  LayoutDashboard,
  Package,
  ShoppingCart,
  Users,
  FolderTree,
  Settings,
  BarChart3,
  CreditCard,
  Truck,
  FileText,
  UserCog,
  Building2,
  SlidersHorizontal,
  Layers,
  Tag,
  Key,
  Shield,
  Library,
  MapPin,
  type LucideIcon,
} from "lucide-react";

export interface NavItem {
  title: string;
  url: string;
  icon: LucideIcon;
  badge?: string | number;
  isActive?: boolean;
  items?: NavSubItem[];
}

export interface NavSubItem {
  title: string;
  url: string;
  badge?: string | number;
}

export interface NavGroup {
  label: string;
  items: NavItem[];
}

export const navigation: NavGroup[] = [
  {
    label: "Overview",
    items: [
      {
        title: "Dashboard",
        url: "/",
        icon: LayoutDashboard,
      },
      {
        title: "Analytics",
        url: "/analytics",
        icon: BarChart3,
      },
    ],
  },
  {
    label: "Catalog",
    items: [
      {
        title: "Products",
        url: "/products",
        icon: Package,
        items: [
          { title: "All Products", url: "/products" },
          { title: "Add Product", url: "/products/new" },
          { title: "Import/Export", url: "/products/import" },
        ],
      },
      {
        title: "Categories",
        url: "/categories",
        icon: FolderTree,
      },
      {
        title: "Attributes",
        url: "/attributes",
        icon: SlidersHorizontal,
      },
      {
        title: "Product Types",
        url: "/product-types",
        icon: Layers,
      },
      {
        title: "Brands",
        url: "/brands",
        icon: Tag,
      },
      {
        title: "Collections",
        url: "/collections",
        icon: Library,
        items: [
          { title: "All Collections", url: "/collections" },
          { title: "Add Collection", url: "/collections/new" },
        ],
      },
    ],
  },
  {
    label: "Sales",
    items: [
      {
        title: "Orders",
        url: "/orders",
        icon: ShoppingCart,
        badge: 12,
        items: [
          { title: "All Orders", url: "/orders" },
          { title: "Pending Approval", url: "/orders/pending", badge: 5 },
          { title: "Processing", url: "/orders/processing" },
          { title: "Completed", url: "/orders/completed" },
        ],
      },
      {
        title: "Returns",
        url: "/returns",
        icon: Truck,
      },
      {
        title: "Invoices",
        url: "/invoices",
        icon: FileText,
      },
    ],
  },
  {
    label: "Customers",
    items: [
      {
        title: "Dealers",
        url: "/customers",
        icon: Building2,
        items: [
          { title: "All Dealers", url: "/customers" },
          { title: "Pending Approval", url: "/customers/pending" },
          { title: "Credit Management", url: "/customers/credits" },
        ],
      },
      {
        title: "Users",
        url: "/users",
        icon: Users,
      },
    ],
  },
  {
    label: "Finance",
    items: [
      {
        title: "Payments",
        url: "/payments",
        icon: CreditCard,
      },
    ],
  },
  {
    label: "System",
    items: [
      {
        title: "Admin Users",
        url: "/admin-users",
        icon: UserCog,
      },
      {
        title: "Roles",
        url: "/roles",
        icon: Shield,
      },
      {
        title: "API Clients",
        url: "/api-clients",
        icon: Key,
      },
      {
        title: "Geo Locations",
        url: "/geo-locations",
        icon: MapPin,
      },
      {
        title: "Settings",
        url: "/settings",
        icon: Settings,
      },
    ],
  },
];
