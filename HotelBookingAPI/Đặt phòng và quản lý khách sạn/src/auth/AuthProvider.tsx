import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { ApiError, api, tokenStore } from "../lib/api";
import type { Role, User } from "../types/api";

interface AuthContextValue {
  user: User | null;
  loading: boolean;
  login: (username: string, password: string) => Promise<User>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<User | null>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  const refreshUser = useCallback(async () => {
    if (!tokenStore.getAccess()) {
      setUser(null);
      return null;
    }
    try {
      const current = await api<User>("/api/Auth/me");
      setUser(current);
      return current;
    } catch (reason) {
      if (reason instanceof ApiError && reason.status === 401) tokenStore.clear();
      setUser(null);
      return null;
    }
  }, []);

  useEffect(() => {
    refreshUser().finally(() => setLoading(false));
  }, [refreshUser]);

  const login = async (username: string, password: string) => {
    const result = await api<Record<string, string>>("/api/Auth/login", {
      method: "POST",
      body: JSON.stringify({ username, password }),
    });
    tokenStore.set(result.accessToken || result.token, result.refreshToken);
    const current = await api<User>("/api/Auth/me");
    setUser(current);
    return current;
  };

  const logout = async () => {
    try {
      const refreshToken = tokenStore.getRefresh();
      if (refreshToken) {
        await api("/api/Auth/logout", {
          method: "POST",
          body: JSON.stringify({ refreshToken }),
        });
      }
    } finally {
      tokenStore.clear();
      setUser(null);
    }
  };

  const value = useMemo(
    () => ({ user, loading, login, logout, refreshUser }),
    [user, loading, refreshUser],
  );
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth must be used inside AuthProvider");
  return value;
}

export const roleHome = (role?: Role) =>
  role === "Admin" ? "/admin" : role === "Staff" ? "/van-hanh" : "/tai-khoan";
