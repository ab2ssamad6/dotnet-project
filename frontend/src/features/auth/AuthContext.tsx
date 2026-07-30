import {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { registerForcedLogoutHandler } from '@/api/client';
import { tokenStore } from '@/api/tokenStore';
import { authService } from '@/services';
import { Role, type LoginRequest, type RegisterRequest, type UserDto } from '@/types';

interface AuthContextValue {
  user: UserDto | null;
  isAuthenticated: boolean;
  initializing: boolean;
  roles: string[];
  hasRole: (...roles: string[]) => boolean;
  isAdmin: boolean;
  isTrainer: boolean;
  isStudent: boolean;
  login: (credentials: LoginRequest) => Promise<UserDto>;
  register: (data: RegisterRequest) => Promise<UserDto>;
  logout: () => Promise<void>;
  updateUser: (user: UserDto) => void;
}

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(() => tokenStore.getUser());
  const [initializing, setInitializing] = useState(true);

  useEffect(() => {
    const stored = tokenStore.getUser();
    const token = tokenStore.getAccessToken();
    if (stored && token) setUser(stored);
    setInitializing(false);

    registerForcedLogoutHandler(() => {
      tokenStore.clear();
      setUser(null);
    });
  }, []);

  const login = useCallback(async (credentials: LoginRequest) => {
    const res = await authService.login(credentials);
    tokenStore.setSession(res.accessToken, res.refreshToken, res.user);
    setUser(res.user);
    return res.user;
  }, []);

  const register = useCallback(async (data: RegisterRequest) => {
    const res = await authService.register(data);
    tokenStore.setSession(res.accessToken, res.refreshToken, res.user);
    setUser(res.user);
    return res.user;
  }, []);

  const logout = useCallback(async () => {
    const refreshToken = tokenStore.getRefreshToken();
    if (refreshToken) {
      await authService.logout(refreshToken).catch(() => undefined);
    }
    tokenStore.clear();
    setUser(null);
  }, []);

  const updateUser = useCallback((next: UserDto) => {
    tokenStore.setUser(next);
    setUser(next);
  }, []);

  const value = useMemo<AuthContextValue>(() => {
    const roles = user?.roles ?? [];
    return {
      user,
      isAuthenticated: !!user,
      initializing,
      roles,
      hasRole: (...check: string[]) => check.some((r) => roles.includes(r)),
      isAdmin: roles.includes(Role.Administrator),
      isTrainer: roles.includes(Role.Trainer),
      isStudent: roles.includes(Role.Student),
      login,
      register,
      logout,
      updateUser,
    };
  }, [user, initializing, login, register, logout, updateUser]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
