import {
    type ReactNode,
    useCallback,
    useEffect,
    useMemo,
    useState,
  } from "react";
  import * as authApi from "./authApi";
  import { AuthContext, type AuthContextValue } from "./AuthContext";
  import { clearTokens, setAccessToken } from "./tokenStorage";
  
  interface AuthProviderProps {
    children: ReactNode;
  }
  
  export function AuthProvider({ children }: AuthProviderProps) {
    const [user, setUser] = useState<authApi.CurrentUser | null>(null);
    const [isLoading, setIsLoading] = useState(true);
  
    const reloadUser = useCallback(async () => {
      try {
        const currentUser = await authApi.getMe();
        setUser(currentUser);
      } catch {
        clearTokens();
        setUser(null);
      }
    }, []);
  
    useEffect(() => {
      async function loadInitialUser() {
        setIsLoading(true);
  
        try {
          await reloadUser();
        } finally {
          setIsLoading(false);
        }
      }
  
      void loadInitialUser();
    }, [reloadUser]);
  
    const login = useCallback(async (request: authApi.LoginRequest) => {
      const response = await authApi.login(request);
  
      setAccessToken(response.accessToken);
  
      setUser({
        userId: response.userId,
        displayName: response.displayName,
        email: response.email,
      });
    }, []);
  
    const register = useCallback(async (request: authApi.RegisterRequest) => {
      const response = await authApi.register(request);
  
      setAccessToken(response.accessToken);
  
      setUser({
        userId: response.userId,
        displayName: response.displayName,
        email: response.email,
      });
    }, []);
  
    const logout = useCallback(async () => {
      try {
        await authApi.logout();
      } finally {
        clearTokens();
        setUser(null);
      }
    }, []);
  
    const value = useMemo<AuthContextValue>(
      () => ({
        user,
        isLoading,
        isAuthenticated: user !== null,
        login,
        register,
        logout,
        reloadUser,
      }),
      [user, isLoading, login, register, logout, reloadUser],
    );
  
    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
  }