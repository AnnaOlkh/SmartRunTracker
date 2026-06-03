import { createContext } from "react";
import type * as authApi from "./authApi";

export interface AuthContextValue {
  user: authApi.CurrentUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (request: authApi.LoginRequest) => Promise<void>;
  register: (request: authApi.RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  reloadUser: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);