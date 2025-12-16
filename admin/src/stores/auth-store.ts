import { create } from "zustand";
import { persist } from "zustand/middleware";
import { apiClient } from "@/lib/api/client";
import { User } from "@/types/entities";
import { decodeJwt, isTokenExpired, getRoleFromPayload } from "@/lib/utils/jwt";
import {
  setAccessToken,
  setRefreshToken,
  getAccessToken,
  getRefreshToken,
  clearAuthCookies,
} from "@/lib/utils/cookies";

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  checkAuth: () => Promise<void>;
  setUser: (user: User) => void;
}

/**
 * Build User object from JWT token claims
 */
function buildUserFromToken(token: string): User {
  const decoded = decodeJwt(token);

  // Construct display name from available fields
  let name = decoded.email;
  if (decoded.firstName) {
    name = `${decoded.firstName} ${decoded.lastName || ""}`.trim();
  } else if (decoded.companyName) {
    name = decoded.companyName;
  }

  return {
    id: decoded.userId || decoded.sub,
    email: decoded.email,
    name,
    role: getRoleFromPayload(decoded),
    isActive: true,
  };
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
          // Backend returns "token", not "accessToken"
          const { token, refreshToken } = response.data;

          // Store tokens in cookies
          setAccessToken(token);
          setRefreshToken(refreshToken);

          // Build user from JWT claims
          const user = buildUserFromToken(token);

          set({ user, isAuthenticated: true, isLoading: false });
        } catch (error) {
          set({ isLoading: false });
          throw error;
        }
      },

      logout: () => {
        clearAuthCookies();
        set({ user: null, isAuthenticated: false });
        window.location.href = "/login";
      },

      checkAuth: async () => {
        const token = getAccessToken();

        // No token - not authenticated
        if (!token) {
          set({ isAuthenticated: false, isLoading: false });
          return;
        }

        // Check if token is expired
        if (isTokenExpired(token)) {
          // Token expired - try to use refresh token
          const refreshTokenValue = getRefreshToken();
          if (!refreshTokenValue) {
            // No refresh token - user needs to login again
            clearAuthCookies();
            set({ user: null, isAuthenticated: false, isLoading: false });
            return;
          }
          // Note: Token refresh will be handled by the axios interceptor
          // on the next API call. For now, mark as not authenticated
          // so the user is redirected to login
          set({ user: null, isAuthenticated: false, isLoading: false });
          return;
        }

        try {
          // Token is valid - build user from JWT claims
          const user = buildUserFromToken(token);
          set({ user, isAuthenticated: true, isLoading: false });
        } catch {
          // Failed to decode token
          clearAuthCookies();
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
