import { apiClient } from "./client";
import { DashboardStats, SalesDataPoint, TopProduct, Order } from "@/types/entities";

export async function getDashboardStats(): Promise<DashboardStats> {
  // For now, return mock data until backend endpoint is available
  return {
    totalRevenue: 125430,
    revenueChange: 12.5,
    totalOrders: 342,
    ordersChange: 8.2,
    totalCustomers: 156,
    customersChange: 4.1,
    activeProducts: 1250,
    productsChange: -2.3,
  };
}

export async function getSalesData(): Promise<SalesDataPoint[]> {
  // Mock data for sales chart
  return [
    { month: "Jan", sales: 45000, orders: 120 },
    { month: "Feb", sales: 52000, orders: 145 },
    { month: "Mar", sales: 48000, orders: 130 },
    { month: "Apr", sales: 61000, orders: 168 },
    { month: "May", sales: 55000, orders: 152 },
    { month: "Jun", sales: 67000, orders: 185 },
    { month: "Jul", sales: 72000, orders: 198 },
    { month: "Aug", sales: 69000, orders: 190 },
    { month: "Sep", sales: 78000, orders: 215 },
    { month: "Oct", sales: 82000, orders: 228 },
    { month: "Nov", sales: 91000, orders: 252 },
    { month: "Dec", sales: 98000, orders: 275 },
  ];
}

export async function getTopProducts(): Promise<TopProduct[]> {
  // Mock data for top products
  return [
    { id: "1", name: "Industrial Motor A500", sku: "MOT-A500", totalSales: 45000, quantitySold: 120 },
    { id: "2", name: "Hydraulic Pump B200", sku: "PMP-B200", totalSales: 38000, quantitySold: 95 },
    { id: "3", name: "Control Panel C100", sku: "CNT-C100", totalSales: 32000, quantitySold: 80 },
    { id: "4", name: "Valve Assembly D50", sku: "VLV-D50", totalSales: 28000, quantitySold: 140 },
    { id: "5", name: "Sensor Unit E25", sku: "SNS-E25", totalSales: 22000, quantitySold: 220 },
  ];
}

export async function getRecentOrders(limit: number = 5): Promise<Order[]> {
  try {
    const response = await apiClient.get(`/orders?pageSize=${limit}&sortBy=CreatedAt&sortDirection=desc`);
    return response.data.items || [];
  } catch {
    // Return mock data if API fails
    return [
      {
        id: "1",
        orderNumber: "ORD-2024-001",
        customerId: "cust-1",
        customerName: "ABC Electronics Ltd.",
        status: "Processing",
        subTotal: 12500,
        discountAmount: 0,
        taxAmount: 2250,
        shippingAmount: 150,
        total: 14900,
        currency: "TRY",
        items: [],
        statusHistory: [],
        createdAt: new Date().toISOString(),
        isDeleted: false,
      },
      {
        id: "2",
        orderNumber: "ORD-2024-002",
        customerId: "cust-2",
        customerName: "XYZ Industrial Co.",
        status: "Pending",
        subTotal: 8750,
        discountAmount: 500,
        taxAmount: 1485,
        shippingAmount: 100,
        total: 9835,
        currency: "TRY",
        items: [],
        statusHistory: [],
        createdAt: new Date(Date.now() - 3600000).toISOString(),
        isDeleted: false,
      },
      {
        id: "3",
        orderNumber: "ORD-2024-003",
        customerId: "cust-3",
        customerName: "Tech Solutions Inc.",
        status: "Shipped",
        subTotal: 22300,
        discountAmount: 1000,
        taxAmount: 3834,
        shippingAmount: 200,
        total: 25334,
        currency: "TRY",
        items: [],
        statusHistory: [],
        createdAt: new Date(Date.now() - 7200000).toISOString(),
        isDeleted: false,
      },
      {
        id: "4",
        orderNumber: "ORD-2024-004",
        customerId: "cust-4",
        customerName: "Global Parts Ltd.",
        status: "Delivered",
        subTotal: 5600,
        discountAmount: 0,
        taxAmount: 1008,
        shippingAmount: 75,
        total: 6683,
        currency: "TRY",
        items: [],
        statusHistory: [],
        createdAt: new Date(Date.now() - 86400000).toISOString(),
        isDeleted: false,
      },
      {
        id: "5",
        orderNumber: "ORD-2024-005",
        customerId: "cust-5",
        customerName: "Metro Engineering",
        status: "Confirmed",
        subTotal: 18900,
        discountAmount: 500,
        taxAmount: 3312,
        shippingAmount: 150,
        total: 21862,
        currency: "TRY",
        items: [],
        statusHistory: [],
        createdAt: new Date(Date.now() - 172800000).toISOString(),
        isDeleted: false,
      },
    ];
  }
}
