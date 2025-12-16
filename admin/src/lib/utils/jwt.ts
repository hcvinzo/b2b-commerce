/**
 * JWT Token Utilities
 * Decode and validate JWT tokens without external dependencies
 */

export interface JwtPayload {
  sub: string;
  email: string;
  jti: string;
  userId: string;
  customerId?: string;
  role: string | string[];
  companyName?: string;
  priceTier?: string;
  isApproved?: string;
  firstName?: string;
  lastName?: string;
  exp: number;
  iat?: number;
  iss: string;
  aud: string;
}

/**
 * Decode a JWT token and extract the payload
 * Note: This does NOT verify the signature - only use for reading claims
 */
export function decodeJwt(token: string): JwtPayload {
  try {
    const base64Url = token.split(".")[1];
    if (!base64Url) {
      throw new Error("Invalid token format");
    }

    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );

    return JSON.parse(jsonPayload);
  } catch {
    throw new Error("Failed to decode JWT token");
  }
}

/**
 * Check if a JWT token has expired
 */
export function isTokenExpired(token: string): boolean {
  try {
    const decoded = decodeJwt(token);
    // exp is in seconds, Date.now() is in milliseconds
    return Date.now() >= decoded.exp * 1000;
  } catch {
    return true; // If we can't decode, treat as expired
  }
}

/**
 * Get the remaining time until token expiration in milliseconds
 * Returns 0 if token is already expired or invalid
 */
export function getTokenExpirationTime(token: string): number {
  try {
    const decoded = decodeJwt(token);
    const expirationMs = decoded.exp * 1000;
    const remainingMs = expirationMs - Date.now();
    return Math.max(0, remainingMs);
  } catch {
    return 0;
  }
}

/**
 * Extract user role from JWT payload
 * Handles both single role string and array of roles
 */
export function getRoleFromPayload(payload: JwtPayload): string {
  if (Array.isArray(payload.role)) {
    return payload.role[0] || "User";
  }
  return payload.role || "User";
}
