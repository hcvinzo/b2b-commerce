import { apiClient, PaginatedResponse } from "./client";
import {
  Customer,
  CustomerFilters,
  UpdateCustomerData,
  CustomerAttribute,
  CustomerAttributeType,
  UpsertCustomerAttributesDto,
  CustomerDocument,
  CreateCustomerDocumentDto,
  FileUploadResponse,
} from "@/types/entities";

const CUSTOMERS_BASE = "/customers";

export async function getCustomers(
  filters: CustomerFilters
): Promise<PaginatedResponse<Customer>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("pageNumber", filters.page.toString());
  if (filters.pageSize) params.append("pageSize", filters.pageSize.toString());
  if (filters.search) params.append("search", filters.search);
  if (filters.status) params.append("status", filters.status);
  if (filters.sortBy) params.append("sortBy", filters.sortBy);
  if (filters.sortOrder) params.append("sortDirection", filters.sortOrder);

  const response = await apiClient.get<PaginatedResponse<Customer>>(
    `${CUSTOMERS_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getCustomer(id: string): Promise<Customer> {
  const response = await apiClient.get<Customer>(`${CUSTOMERS_BASE}/${id}`);
  return response.data;
}

export async function approveCustomer(id: string): Promise<void> {
  await apiClient.post(`${CUSTOMERS_BASE}/${id}/approve`);
}

export async function activateCustomer(id: string): Promise<void> {
  await apiClient.post(`${CUSTOMERS_BASE}/${id}/activate`);
}

export async function deactivateCustomer(id: string): Promise<void> {
  await apiClient.post(`${CUSTOMERS_BASE}/${id}/deactivate`);
}

export async function updateCustomer(
  id: string,
  data: UpdateCustomerData
): Promise<Customer> {
  const response = await apiClient.put<Customer>(`${CUSTOMERS_BASE}/${id}`, data);
  return response.data;
}

export async function deleteCustomer(id: string): Promise<void> {
  await apiClient.delete(`${CUSTOMERS_BASE}/${id}`);
}

// Customer Attributes API
export async function getCustomerAttributes(
  customerId: string,
  type?: CustomerAttributeType
): Promise<CustomerAttribute[]> {
  const params = type ? `?type=${type}` : "";
  const response = await apiClient.get<CustomerAttribute[]>(
    `${CUSTOMERS_BASE}/${customerId}/attributes${params}`
  );
  return response.data;
}

export async function upsertCustomerAttributes(
  customerId: string,
  data: UpsertCustomerAttributesDto
): Promise<CustomerAttribute[]> {
  const response = await apiClient.put<CustomerAttribute[]>(
    `${CUSTOMERS_BASE}/${customerId}/attributes`,
    data
  );
  return response.data;
}

export async function deleteCustomerAttribute(
  customerId: string,
  attributeId: string
): Promise<void> {
  await apiClient.delete(`${CUSTOMERS_BASE}/${customerId}/attributes/${attributeId}`);
}

// Customer Documents API
export async function getCustomerDocuments(
  customerId: string
): Promise<CustomerDocument[]> {
  const response = await apiClient.get<CustomerDocument[]>(
    `${CUSTOMERS_BASE}/${customerId}/documents`
  );
  return response.data;
}

export async function getCustomerDocument(
  customerId: string,
  documentId: string
): Promise<CustomerDocument> {
  const response = await apiClient.get<CustomerDocument>(
    `${CUSTOMERS_BASE}/${customerId}/documents/${documentId}`
  );
  return response.data;
}

export async function createCustomerDocument(
  customerId: string,
  data: CreateCustomerDocumentDto
): Promise<CustomerDocument> {
  const response = await apiClient.post<CustomerDocument>(
    `${CUSTOMERS_BASE}/${customerId}/documents`,
    data
  );
  return response.data;
}

export async function deleteCustomerDocument(
  customerId: string,
  documentId: string
): Promise<void> {
  await apiClient.delete(`${CUSTOMERS_BASE}/${customerId}/documents/${documentId}`);
}

export async function uploadDocument(file: File): Promise<FileUploadResponse> {
  const formData = new FormData();
  formData.append("file", file);

  const response = await apiClient.post<FileUploadResponse>(
    "/files/upload/document",
    formData,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }
  );
  return response.data;
}
