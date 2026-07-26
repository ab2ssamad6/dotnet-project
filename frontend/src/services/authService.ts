import { http } from '@/api/client';
import type {
  AuthResponse,
  ChangePasswordRequest,
  ForgotPasswordRequest,
  LoginRequest,
  RegisterRequest,
  ResetPasswordRequest,
  VerifyEmailRequest,
} from '@/types';

export const authService = {
  register: (body: RegisterRequest) => http.post<AuthResponse>('/api/auth/register', body),
  login: (body: LoginRequest) => http.post<AuthResponse>('/api/auth/login', body),
  refresh: (refreshToken: string) => http.post<AuthResponse>('/api/auth/refresh', { refreshToken }),
  logout: (refreshToken: string) => http.post<void>('/api/auth/logout', { refreshToken }),
  forgotPassword: (body: ForgotPasswordRequest) => http.post<void>('/api/auth/forgot-password', body),
  resetPassword: (body: ResetPasswordRequest) => http.post<void>('/api/auth/reset-password', body),
  verifyEmail: (body: VerifyEmailRequest) => http.post<void>('/api/auth/verify-email', body),
  changePassword: (body: ChangePasswordRequest) => http.post<void>('/api/auth/change-password', body),
};
