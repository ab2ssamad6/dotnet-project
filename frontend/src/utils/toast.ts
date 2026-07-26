import toast from 'react-hot-toast';
import { toApiError } from '@/api/errors';

/** Centralized toast helpers so styling/behavior stays consistent. */
export const notify = {
  success: (message: string) => toast.success(message),
  error: (message: string) => toast.error(message),
  loading: (message: string) => toast.loading(message),
  dismiss: (id?: string) => toast.dismiss(id),
  /** Show a normalized error message from any thrown value. */
  apiError: (error: unknown, fallback?: string) => {
    const apiErr = toApiError(error);
    toast.error(apiErr.message || fallback || 'Something went wrong.');
    return apiErr;
  },
};
