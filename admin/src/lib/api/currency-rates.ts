import { apiClient } from "./client";
import {
  CurrencyRate,
  CurrencyRateListItem,
  CreateCurrencyRateDto,
  UpdateCurrencyRateDto,
  CurrencyRateFilters,
} from "@/types/entities";

const CURRENCY_RATES_BASE = "/admin/currency-rates";

export async function getCurrencyRates(
  filters: CurrencyRateFilters = {}
): Promise<CurrencyRateListItem[]> {
  const params = new URLSearchParams();

  if (filters.fromCurrency) {
    params.append("fromCurrency", filters.fromCurrency);
  }

  if (filters.toCurrency) {
    params.append("toCurrency", filters.toCurrency);
  }

  if (filters.activeOnly !== undefined) {
    params.append("activeOnly", String(filters.activeOnly));
  }

  const queryString = params.toString();
  const url = queryString ? `${CURRENCY_RATES_BASE}?${queryString}` : CURRENCY_RATES_BASE;

  const response = await apiClient.get<CurrencyRateListItem[]>(url);
  return response.data;
}

export async function getCurrencyRate(id: string): Promise<CurrencyRate> {
  const response = await apiClient.get<CurrencyRate>(`${CURRENCY_RATES_BASE}/${id}`);
  return response.data;
}

export async function getCurrencyRatePair(
  fromCurrency: string,
  toCurrency: string
): Promise<CurrencyRate> {
  const params = new URLSearchParams({
    fromCurrency,
    toCurrency,
  });
  const response = await apiClient.get<CurrencyRate>(
    `${CURRENCY_RATES_BASE}/pair?${params.toString()}`
  );
  return response.data;
}

export async function createCurrencyRate(data: CreateCurrencyRateDto): Promise<CurrencyRate> {
  const response = await apiClient.post<CurrencyRate>(CURRENCY_RATES_BASE, data);
  return response.data;
}

export async function updateCurrencyRate(
  id: string,
  data: UpdateCurrencyRateDto
): Promise<CurrencyRate> {
  const response = await apiClient.put<CurrencyRate>(`${CURRENCY_RATES_BASE}/${id}`, data);
  return response.data;
}

export async function deleteCurrencyRate(id: string): Promise<void> {
  await apiClient.delete(`${CURRENCY_RATES_BASE}/${id}`);
}

export async function activateCurrencyRate(id: string): Promise<CurrencyRate> {
  const response = await apiClient.post<CurrencyRate>(`${CURRENCY_RATES_BASE}/${id}/activate`);
  return response.data;
}

export async function deactivateCurrencyRate(id: string): Promise<CurrencyRate> {
  const response = await apiClient.post<CurrencyRate>(`${CURRENCY_RATES_BASE}/${id}/deactivate`);
  return response.data;
}
