import { apiClient } from "./client";
import {
  Currency,
  CurrencyListItem,
  CreateCurrencyDto,
  UpdateCurrencyDto,
  CurrencyFilters,
} from "@/types/entities";

const CURRENCIES_BASE = "/admin/currencies";

export async function getCurrencies(
  filters: CurrencyFilters = {}
): Promise<CurrencyListItem[]> {
  const params = new URLSearchParams();

  if (filters.activeOnly !== undefined) {
    params.append("activeOnly", String(filters.activeOnly));
  }

  const queryString = params.toString();
  const url = queryString ? `${CURRENCIES_BASE}?${queryString}` : CURRENCIES_BASE;

  const response = await apiClient.get<CurrencyListItem[]>(url);
  return response.data;
}

export async function getCurrency(id: string): Promise<Currency> {
  const response = await apiClient.get<Currency>(`${CURRENCIES_BASE}/${id}`);
  return response.data;
}

export async function getCurrencyByCode(code: string): Promise<Currency> {
  const response = await apiClient.get<Currency>(
    `${CURRENCIES_BASE}/by-code/${encodeURIComponent(code)}`
  );
  return response.data;
}

export async function getDefaultCurrency(): Promise<Currency> {
  const response = await apiClient.get<Currency>(`${CURRENCIES_BASE}/default`);
  return response.data;
}

export async function createCurrency(data: CreateCurrencyDto): Promise<Currency> {
  const response = await apiClient.post<Currency>(CURRENCIES_BASE, data);
  return response.data;
}

export async function updateCurrency(
  id: string,
  data: UpdateCurrencyDto
): Promise<Currency> {
  const response = await apiClient.put<Currency>(`${CURRENCIES_BASE}/${id}`, data);
  return response.data;
}

export async function deleteCurrency(id: string): Promise<void> {
  await apiClient.delete(`${CURRENCIES_BASE}/${id}`);
}

export async function activateCurrency(id: string): Promise<Currency> {
  const response = await apiClient.post<Currency>(`${CURRENCIES_BASE}/${id}/activate`);
  return response.data;
}

export async function deactivateCurrency(id: string): Promise<Currency> {
  const response = await apiClient.post<Currency>(`${CURRENCIES_BASE}/${id}/deactivate`);
  return response.data;
}

export async function setDefaultCurrency(id: string): Promise<Currency> {
  const response = await apiClient.post<Currency>(`${CURRENCIES_BASE}/${id}/set-default`);
  return response.data;
}
