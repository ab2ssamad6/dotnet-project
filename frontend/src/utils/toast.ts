import toast from 'react-hot-toast';
import { toApiError } from '@/api/errors';

export const notify = {
  success: (message: string) => toast.success(message),
  error: (message: string) => toast.error(message),
  loading: (message: string) => toast.loading(message),
  dismiss: (id?: string) => toast.dismiss(id),
  apiError: (error: unknown, fallback?: string) => {
    const apiErr = toApiError(error);
    toast.error(apiErr.message || fallback || 'Something went wrong.');
    return apiErr;
  },
};
