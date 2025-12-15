import { apiClient, PaginatedResponse } from "./client";
import { Order, OrderFilters, OrderStatus } from "@/types/entities";

const ORDERS_BASE = "/orders";

export async function getOrders(
  filters: OrderFilters
): Promise<PaginatedResponse<Order>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("pageNumber", filters.page.toString());
  if (filters.pageSize) params.append("pageSize", filters.pageSize.toString());
  if (filters.search) params.append("search", filters.search);
  if (filters.status) params.append("status", filters.status);
  if (filters.customerId) params.append("customerId", filters.customerId);
  if (filters.fromDate) params.append("fromDate", filters.fromDate);
  if (filters.toDate) params.append("toDate", filters.toDate);
  if (filters.sortBy) params.append("sortBy", filters.sortBy);
  if (filters.sortOrder) params.append("sortDirection", filters.sortOrder);

  const response = await apiClient.get<PaginatedResponse<Order>>(
    `${ORDERS_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getOrder(id: string): Promise<Order> {
  const response = await apiClient.get<Order>(`${ORDERS_BASE}/${id}`);
  return response.data;
}

export async function updateOrderStatus(
  id: string,
  status: OrderStatus,
  notes?: string
): Promise<Order> {
  const response = await apiClient.patch<Order>(`${ORDERS_BASE}/${id}/status`, {
    status,
    notes,
  });
  return response.data;
}

export async function cancelOrder(id: string, reason?: string): Promise<void> {
  await apiClient.post(`${ORDERS_BASE}/${id}/cancel`, { reason });
}
