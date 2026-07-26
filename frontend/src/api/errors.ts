import { AxiosError } from 'axios';
import type { ProblemDetails } from '@/types';

/** A normalized API error the UI can rely on. */
export class ApiError extends Error {
  status: number;
  detail?: string;
  title?: string;
  fieldErrors?: Record<string, string[]>;

  constructor(message: string, status: number, options?: Partial<ApiError>) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.detail = options?.detail;
    this.title = options?.title;
    this.fieldErrors = options?.fieldErrors;
  }
}

/** Convert any thrown value (usually an AxiosError) into a normalized ApiError. */
export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error;

  if (error instanceof AxiosError) {
    const status = error.response?.status ?? 0;
    const data = error.response?.data as ProblemDetails | string | undefined;

    if (status === 0 || error.code === 'ERR_NETWORK') {
      return new ApiError('Unable to reach the server. Check your connection and try again.', 0);
    }

    if (typeof data === 'string') {
      return new ApiError(data || error.message, status);
    }

    if (data && typeof data === 'object') {
      const message = data.detail || data.title || defaultMessageForStatus(status);
      return new ApiError(message, status, {
        detail: data.detail,
        title: data.title,
        fieldErrors: data.errors,
      });
    }

    return new ApiError(error.message || defaultMessageForStatus(status), status);
  }

  if (error instanceof Error) return new ApiError(error.message, 0);
  return new ApiError('An unexpected error occurred.', 0);
}

function defaultMessageForStatus(status: number): string {
  switch (status) {
    case 400:
      return 'The request was invalid.';
    case 401:
      return 'Your session has expired. Please sign in again.';
    case 403:
      return 'You do not have permission to perform this action.';
    case 404:
      return 'The requested resource was not found.';
    case 409:
      return 'This action conflicts with the current state.';
    case 429:
      return 'Too many requests. Please slow down and try again.';
    case 500:
      return 'A server error occurred. Please try again later.';
    default:
      return 'Something went wrong.';
  }
}
