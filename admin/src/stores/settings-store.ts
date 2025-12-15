import { create } from "zustand";
import { persist } from "zustand/middleware";

interface TableSettings {
  pageSize: number;
  density: "comfortable" | "compact";
}

interface SettingsState {
  locale: string;
  tableSettings: TableSettings;
  setLocale: (locale: string) => void;
  setTableSettings: (settings: Partial<TableSettings>) => void;
}

export const useSettingsStore = create<SettingsState>()(
  persist(
    (set) => ({
      locale: "tr",
      tableSettings: {
        pageSize: 10,
        density: "comfortable",
      },
      setLocale: (locale) => set({ locale }),
      setTableSettings: (settings) =>
        set((state) => ({
          tableSettings: { ...state.tableSettings, ...settings },
        })),
    }),
    {
      name: "admin-settings",
    }
  )
);
