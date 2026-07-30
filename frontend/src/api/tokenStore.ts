import { STORAGE_KEYS } from '@/constants/config';
import type { UserDto } from '@/types';

export const tokenStore = {
  getAccessToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.accessToken);
  },
  getRefreshToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.refreshToken);
  },
  getUser(): UserDto | null {
    const raw = localStorage.getItem(STORAGE_KEYS.user);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UserDto;
    } catch {
      return null;
    }
  },
  setSession(accessToken: string, refreshToken: string, user: UserDto): void {
    localStorage.setItem(STORAGE_KEYS.accessToken, accessToken);
    localStorage.setItem(STORAGE_KEYS.refreshToken, refreshToken);
    localStorage.setItem(STORAGE_KEYS.user, JSON.stringify(user));
  },
  setUser(user: UserDto): void {
    localStorage.setItem(STORAGE_KEYS.user, JSON.stringify(user));
  },
  clear(): void {
    localStorage.removeItem(STORAGE_KEYS.accessToken);
    localStorage.removeItem(STORAGE_KEYS.refreshToken);
    localStorage.removeItem(STORAGE_KEYS.user);
  },
};
