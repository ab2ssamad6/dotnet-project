import axios, {
  AxiosError,
  AxiosHeaders,
  type AxiosInstance,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from 'axios';
import { API_BASE_URL } from '@/constants/config';
import type { AuthResponse } from '@/types';
import { tokenStore } from './tokenStore';
import { toApiError } from './errors';

interface RetryConfig extends InternalAxiosRequestConfig {
  _retried?: boolean;
  _retryCount?: number;
}

let onForcedLogout: (() => void) | null = null;
export function registerForcedLogoutHandler(handler: () => void): void {
  onForcedLogout = handler;
}

const bareClient = axios.create({ baseURL: API_BASE_URL });

export const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30_000,
});

apiClient.interceptors.request.use((config) => {
  const token = tokenStore.getAccessToken();
  if (token) {
    const headers = AxiosHeaders.from(config.headers);
    headers.set('Authorization', `Bearer ${token}`);
    config.headers = headers;
  }
  return config;
});

let refreshPromise: Promise<string> | null = null;

async function refreshAccessToken(): Promise<string> {
  const refreshToken = tokenStore.getRefreshToken();
  if (!refreshToken) throw new Error('No refresh token available.');

  const { data } = await bareClient.post<AuthResponse>('/api/auth/refresh', { refreshToken });
  tokenStore.setSession(data.accessToken, data.refreshToken, data.user);
  return data.accessToken;
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const config = error.config as RetryConfig | undefined;
    const status = error.response?.status;

    const isAuthCall = config?.url?.includes('/api/auth/');
    if (status === 401 && config && !config._retried && !isAuthCall && tokenStore.getRefreshToken()) {
      config._retried = true;
      try {
        refreshPromise ??= refreshAccessToken().finally(() => {
          refreshPromise = null;
        });
        const newToken = await refreshPromise;
        const headers = AxiosHeaders.from(config.headers);
        headers.set('Authorization', `Bearer ${newToken}`);
        config.headers = headers;
        return apiClient(config);
      } catch {
        tokenStore.clear();
        onForcedLogout?.();
        return Promise.reject(toApiError(error));
      }
    }

    const isGet = (config?.method ?? 'get').toLowerCase() === 'get';
    const transient = status === undefined || status >= 500;
    if (config && isGet && transient) {
      config._retryCount = (config._retryCount ?? 0) + 1;
      if (config._retryCount <= 2) {
        const backoff = 300 * config._retryCount;
        await new Promise((resolve) => setTimeout(resolve, backoff));
        return apiClient(config);
      }
    }

    return Promise.reject(toApiError(error));
  },
);

export const http = {
  get: <T>(url: string, config?: AxiosRequestConfig) => apiClient.get<T>(url, config).then((r) => r.data),
  post: <T>(url: string, body?: unknown, config?: AxiosRequestConfig) =>
    apiClient.post<T>(url, body, config).then((r) => r.data),
  put: <T>(url: string, body?: unknown, config?: AxiosRequestConfig) =>
    apiClient.put<T>(url, body, config).then((r) => r.data),
  patch: <T>(url: string, body?: unknown, config?: AxiosRequestConfig) =>
    apiClient.patch<T>(url, body, config).then((r) => r.data),
  delete: <T = void>(url: string, config?: AxiosRequestConfig) =>
    apiClient.delete<T>(url, config).then((r) => r.data),
};
