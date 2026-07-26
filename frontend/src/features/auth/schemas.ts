import { z } from 'zod';
import { Role } from '@/types';

// Mirror the backend password policy (Lms.Application/Validation/AuthValidators.cs).
const password = z
  .string()
  .min(8, 'Password must be at least 8 characters.')
  .regex(/[A-Z]/, 'Must contain an uppercase letter.')
  .regex(/[a-z]/, 'Must contain a lowercase letter.')
  .regex(/[0-9]/, 'Must contain a digit.')
  .regex(/[^a-zA-Z0-9]/, 'Must contain a non-alphanumeric character.');

export const loginSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email.'),
  password: z.string().min(1, 'Password is required.'),
});
export type LoginForm = z.infer<typeof loginSchema>;

export const registerSchema = z
  .object({
    firstName: z.string().min(1, 'First name is required.').max(100),
    lastName: z.string().min(1, 'Last name is required.').max(100),
    email: z.string().min(1, 'Email is required.').email('Enter a valid email.'),
    role: z.enum([Role.Student, Role.Trainer]),
    password,
    confirmPassword: z.string().min(1, 'Please confirm your password.'),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match.',
    path: ['confirmPassword'],
  });
export type RegisterForm = z.infer<typeof registerSchema>;

export const forgotPasswordSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email.'),
});
export type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>;

export const resetPasswordSchema = z
  .object({
    email: z.string().min(1, 'Email is required.').email('Enter a valid email.'),
    token: z.string().min(1, 'Reset token is required.'),
    newPassword: password,
    confirmPassword: z.string().min(1, 'Please confirm your password.'),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Passwords do not match.',
    path: ['confirmPassword'],
  });
export type ResetPasswordForm = z.infer<typeof resetPasswordSchema>;

export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, 'Current password is required.'),
    newPassword: password,
    confirmPassword: z.string().min(1, 'Please confirm your password.'),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Passwords do not match.',
    path: ['confirmPassword'],
  })
  .refine((data) => data.newPassword !== data.currentPassword, {
    message: 'New password must differ from the current password.',
    path: ['newPassword'],
  });
export type ChangePasswordForm = z.infer<typeof changePasswordSchema>;
