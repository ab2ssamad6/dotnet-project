export const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '');

export const STORAGE_KEYS = {
  accessToken: 'lms.accessToken',
  refreshToken: 'lms.refreshToken',
  user: 'lms.user',
} as const;

export const DEFAULT_PAGE_SIZE = 10;
export const PAGE_SIZE_OPTIONS = [10, 20, 50];
