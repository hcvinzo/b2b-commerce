import * as XLSX from "xlsx";
import { Category } from "@/types/entities";

// Export column definition
export interface ExportColumn<T> {
  key: keyof T | string;
  header: string;
  format?: (value: unknown, row: T) => string;
}

// Export strategy enum for future use
export type ExportStrategy = "client" | "backend-sync" | "backend-async";

// Flattened category for tabular export
export interface FlatCategory {
  id: string;
  name: string;
  parentName: string;
  path: string;
  description: string;
  displayOrder: number;
  isActive: string;
  productCount: number;
  level: number;
}

/**
 * Flatten hierarchical categories for tabular export
 */
export function flattenCategories(
  categories: Category[],
  parentName = "",
  parentPath = "",
  level = 0
): FlatCategory[] {
  const result: FlatCategory[] = [];

  for (const category of categories) {
    const currentPath = parentPath
      ? `${parentPath} > ${category.name}`
      : category.name;

    result.push({
      id: category.id,
      name: category.name,
      parentName,
      path: currentPath,
      description: category.description || "",
      displayOrder: category.displayOrder,
      isActive: category.isActive ? "Yes" : "No",
      productCount: category.productCount || 0,
      level,
    });

    // Recursively flatten children
    if (category.children && category.children.length > 0) {
      result.push(
        ...flattenCategories(
          category.children,
          category.name,
          currentPath,
          level + 1
        )
      );
    }
  }

  return result;
}

/**
 * Get nested value from object using dot notation
 */
function getNestedValue(obj: object, key: string): unknown {
  return key.split(".").reduce<unknown>((o, k) => {
    if (o && typeof o === "object" && k in o) {
      return (o as Record<string, unknown>)[k];
    }
    return undefined;
  }, obj);
}

/**
 * Generate CSV string from data
 */
export function generateCSV<T extends object>(
  data: T[],
  columns: ExportColumn<T>[]
): string {
  // BOM for UTF-8 encoding (helps Excel recognize Turkish characters)
  const BOM = "\uFEFF";

  // Header row
  const headers = columns.map((col) => escapeCSVField(col.header)).join(",");

  // Data rows
  const rows = data.map((row) => {
    return columns
      .map((col) => {
        const value = getNestedValue(row, col.key as string);
        const formatted = col.format
          ? col.format(value, row)
          : value !== undefined && value !== null
            ? String(value)
            : "";
        return escapeCSVField(formatted);
      })
      .join(",");
  });

  return BOM + [headers, ...rows].join("\n");
}

/**
 * Escape CSV field value
 */
function escapeCSVField(value: string): string {
  // If value contains comma, quote, or newline, wrap in quotes and escape quotes
  if (value.includes(",") || value.includes('"') || value.includes("\n")) {
    return `"${value.replace(/"/g, '""')}"`;
  }
  return value;
}

/**
 * Generate Excel file using xlsx library
 */
export function generateExcel<T extends object>(
  data: T[],
  columns: ExportColumn<T>[],
  sheetName: string
): Blob {
  // Create workbook and worksheet
  const wb = XLSX.utils.book_new();

  // Prepare data with headers
  const wsData = [
    columns.map((col) => col.header),
    ...data.map((row) =>
      columns.map((col) => {
        const value = getNestedValue(row, col.key as string);
        if (col.format) {
          return col.format(value, row);
        }
        return value !== undefined && value !== null ? value : "";
      })
    ),
  ];

  const ws = XLSX.utils.aoa_to_sheet(wsData);

  // Auto-size columns based on content
  const colWidths = columns.map((col, colIndex) => {
    let maxWidth = col.header.length;
    data.forEach((row) => {
      const value = getNestedValue(row, col.key as string);
      const formatted = col.format
        ? col.format(value, row)
        : value !== undefined && value !== null
          ? String(value)
          : "";
      maxWidth = Math.max(maxWidth, formatted.length);
    });
    return { wch: Math.min(maxWidth + 2, 50) }; // Max width of 50
  });
  ws["!cols"] = colWidths;

  // Add worksheet to workbook
  XLSX.utils.book_append_sheet(wb, ws, sheetName);

  // Generate buffer
  const wbout = XLSX.write(wb, { bookType: "xlsx", type: "array" });

  return new Blob([wbout], {
    type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  });
}

/**
 * Trigger file download in browser
 */
export function downloadFile(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}

/**
 * High-level function to export data to CSV and download
 */
export function exportToCSV<T extends object>(
  data: T[],
  columns: ExportColumn<T>[],
  filename: string
): void {
  const csv = generateCSV(data, columns);
  const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  downloadFile(blob, `${filename}.csv`);
}

/**
 * High-level function to export data to Excel and download
 */
export function exportToExcel<T extends object>(
  data: T[],
  columns: ExportColumn<T>[],
  filename: string,
  sheetName = "Sheet1"
): void {
  const blob = generateExcel(data, columns, sheetName);
  downloadFile(blob, `${filename}.xlsx`);
}
