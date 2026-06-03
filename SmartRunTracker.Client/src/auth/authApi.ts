import { apiRequest } from "../api/client";

export interface AuthResponse {
  userId: number;
  displayName: string;
  email: string;
  accessToken: string;
  accessTokenExpiresAt: string;
}

export interface CurrentUser {
  userId: number;
  displayName: string;
  email: string;
}

export interface RegisterRequest {
  displayName: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export function register(request: RegisterRequest): Promise<AuthResponse> {
  return apiRequest<AuthResponse>("/auth/register", {
    method: "POST",
    body: request,
    skipAuth: true,
    skipRefresh: true,
  });
}

export function login(request: LoginRequest): Promise<AuthResponse> {
  return apiRequest<AuthResponse>("/auth/login", {
    method: "POST",
    body: request,
    skipAuth: true,
    skipRefresh: true,
  });
}

export function getMe(): Promise<CurrentUser> {
  return apiRequest<CurrentUser>("/auth/me");
}

export function logout(): Promise<void> {
  return apiRequest<void>("/auth/logout", {
    method: "POST",
    skipAuth: true,
    skipRefresh: true,
  });
}