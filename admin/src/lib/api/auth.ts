import { apiClient } from "./client";

export interface LoginRequest {
  email: string;
  password: string;
}

// Response from backend /auth/login endpoint
export interface LoginResponse {
  token: string; // Backend returns "token", not "accessToken"
  refreshToken: string;
  expiresAt: string;
  customerId: string;
  email: string;
  companyName: string;
  isApproved: boolean;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// Response from backend /auth/refresh endpoint
export interface RefreshTokenResponse {
  token: string; // Backend returns "token", not "accessToken"
  refreshToken: string;
  expiresAt: string;
  customerId: string;
  email: string;
  companyName: string;
  isApproved: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export async function login(data: LoginRequest): Promise<LoginResponse> {
  const response = await apiClient.post<LoginResponse>("/auth/login", data);
  return response.data;
}

export async function refreshToken(
  data: RefreshTokenRequest
): Promise<RefreshTokenResponse> {
  const response = await apiClient.post<RefreshTokenResponse>(
    "/auth/refresh",
    data
  );
  return response.data;
}

export async function logout(): Promise<void> {
  await apiClient.post("/auth/logout");
}

export async function changePassword(data: ChangePasswordRequest): Promise<void> {
  await apiClient.post("/auth/change-password", data);
}

export async function forgotPassword(email: string): Promise<void> {
  await apiClient.post("/auth/forgot-password", { email });
}

export async function resetPassword(data: ResetPasswordRequest): Promise<void> {
  await apiClient.post("/auth/reset-password", data);
}
