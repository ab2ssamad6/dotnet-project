import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { Button, Card, Input } from '@/components/ui';
import { zodResolver } from '@/utils/zodResolver';
import { resetPasswordSchema, type ResetPasswordForm } from '@/features/auth/schemas';
import { authService } from '@/services';
import { notify } from '@/utils/toast';

export function ResetPasswordPage() {
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordForm>({ resolver: zodResolver(resetPasswordSchema) });

  // Pre-fill from a reset link (e.g. /reset-password?email=..&token=..).
  useEffect(() => {
    const email = params.get('email');
    const token = params.get('token');
    if (email) setValue('email', email);
    if (token) setValue('token', token);
  }, [params, setValue]);

  const onSubmit = async (data: ResetPasswordForm) => {
    try {
      await authService.resetPassword(data);
      notify.success('Password reset successfully. Please sign in.');
      navigate('/login', { replace: true });
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <Card className="p-8">
      <h1 className="text-2xl font-bold text-slate-900">Reset password</h1>
      <p className="mt-1 text-sm text-slate-500">Enter the token you received and choose a new password.</p>
      <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
        <Input label="Email" type="email" error={errors.email?.message} required {...register('email')} />
        <Input
          label="Reset token"
          placeholder="Paste the token from your email"
          error={errors.token?.message}
          required
          {...register('token')}
        />
        <Input
          label="New password"
          type="password"
          hint="8+ chars with upper, lower, digit & symbol."
          error={errors.newPassword?.message}
          required
          {...register('newPassword')}
        />
        <Input
          label="Confirm new password"
          type="password"
          error={errors.confirmPassword?.message}
          required
          {...register('confirmPassword')}
        />
        <Button type="submit" fullWidth size="lg" loading={isSubmitting}>
          Reset password
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-slate-500">
        <Link to="/login" className="font-medium text-brand-600 hover:underline">
          Back to sign in
        </Link>
      </p>
    </Card>
  );
}
