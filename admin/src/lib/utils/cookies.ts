import Cookies from "js-cookie";

/**
 * Check if we're running in a secure context (HTTPS)
 */
function isSecureContext(): boolean {
  if (typeof window === "undefined") return false;
  return window.location.protocol === "https:";
}

/**
 * Get cookie options based on environment
 * In development (HTTP), we can't use secure cookies
 */
export function getCookieOptions(): Cookies.CookieAttributes {
  return {
    secure: isSecureContext(),
    sameSite: "strict",
  };
}

/**
 * Set auth token cookie
 */
export function setAccessToken(token: string): void {
  Cookies.set("accessToken", token, getCookieOptions());
}

/**
 * Set refresh token cookie
 */
export function setRefreshToken(token: string): void {
  Cookies.set("refreshToken", token, getCookieOptions());
}

/**
 * Get access token from cookie
 */
export function getAccessToken(): string | undefined {
  return Cookies.get("accessToken");
}

/**
 * Get refresh token from cookie
 */
export function getRefreshToken(): string | undefined {
  return Cookies.get("refreshToken");
}

/**
 * Clear all auth cookies
 */
export function clearAuthCookies(): void {
  Cookies.remove("accessToken");
  Cookies.remove("refreshToken");
}
