import { create } from "zustand";
import { persist } from "zustand/middleware";
import Cookies from "js-cookie";
import { apiClient } from "@/lib/api/client";
import { User } from "@/types/entities";

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  checkAuth: () => Promise<void>;
  setUser: (user: User) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,
      isLoading: true,

      login: async (email: string, password: string) => {
        set({ isLoading: true });
        try {
          const response = await apiClient.post("/auth/login", {
            email,
            password,
          });
          const { accessToken, refreshToken, user } = response.data;

          Cookies.set("accessToken", accessToken, {
            secure: true,
            sameSite: "strict",
          });
          Cookies.set("refreshToken", refreshToken, {
            secure: true,
            sameSite: "strict",
          });

          set({ user, isAuthenticated: true, isLoading: false });
        } catch (error) {
          set({ isLoading: false });
          throw error;
        }
      },

      logout: () => {
        Cookies.remove("accessToken");
        Cookies.remove("refreshToken");
        set({ user: null, isAuthenticated: false });
        window.location.href = "/login";
      },

      checkAuth: async () => {
        const token = Cookies.get("accessToken");
        if (!token) {
          set({ isAuthenticated: false, isLoading: false });
          return;
        }

        try {
          const response = await apiClient.get("/auth/me");
          set({ user: response.data, isAuthenticated: true, isLoading: false });
        } catch {
          set({ user: null, isAuthenticated: false, isLoading: false });
        }
      },

      setUser: (user) => set({ user }),
    }),
    {
      name: "auth-storage",
      partialize: (state) => ({ user: state.user }),
    }
  )
);
