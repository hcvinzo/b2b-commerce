import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

// RTL-aware spacing utilities
export function rtlAware(ltrClass: string, rtlClass: string) {
  return `${ltrClass} rtl:${rtlClass}`;
}

// Common RTL transformations
export const rtl = {
  // Margins
  ml: (value: string) => `ml-${value} rtl:mr-${value} rtl:ml-0`,
  mr: (value: string) => `mr-${value} rtl:ml-${value} rtl:mr-0`,
  // Padding
  pl: (value: string) => `pl-${value} rtl:pr-${value} rtl:pl-0`,
  pr: (value: string) => `pr-${value} rtl:pl-${value} rtl:pr-0`,
  // Text alignment
  textLeft: "text-left rtl:text-right",
  textRight: "text-right rtl:text-left",
  // Positioning
  left: (value: string) => `left-${value} rtl:right-${value} rtl:left-auto`,
  right: (value: string) => `right-${value} rtl:left-${value} rtl:right-auto`,
  // Borders
  borderL: (value: string) =>
    `border-l-${value} rtl:border-r-${value} rtl:border-l-0`,
  borderR: (value: string) =>
    `border-r-${value} rtl:border-l-${value} rtl:border-r-0`,
  // Rounded corners
  roundedL: (value: string) =>
    `rounded-l-${value} rtl:rounded-r-${value} rtl:rounded-l-none`,
  roundedR: (value: string) =>
    `rounded-r-${value} rtl:rounded-l-${value} rtl:rounded-r-none`,
};

// Format currency
export function formatCurrency(
  amount: number,
  currency: string = "TRY"
): string {
  return new Intl.NumberFormat("tr-TR", {
    style: "currency",
    currency,
  }).format(amount);
}

// Format date
export function formatDate(date: string | Date): string {
  return new Intl.DateTimeFormat("tr-TR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(date));
}

// Format date time
export function formatDateTime(date: string | Date): string {
  return new Intl.DateTimeFormat("tr-TR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(date));
}
