import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { Button, Card, Icons, Input } from '@/components/ui';
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

  useEffect(() => {
    const email = params.get('email');
    const token = params.get('token');
    if (email) setValue('email', email);
    if (token) setValue('token', token);
  }, [params, setValue]);

  const onSubmit = async (data: ResetPasswordForm) => {
    try {
      await authService.resetPassword(data);
      notify.success('Password updated. You can sign in now.');
      navigate('/login', { replace: true });
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <Card className="p-8 shadow-raised sm:p-9">
      <p className="eyebrow">Account recovery</p>
      <h1 className="mt-2 font-display text-[27px] font-semibold tracking-[-0.02em] text-ink-900">
        Choose a new password
      </h1>
      <p className="mt-2 text-sm text-ink-500">Paste the token from your email, then set the password you'll use.</p>
      <form onSubmit={handleSubmit(onSubmit)} className="mt-7 space-y-4">
        <Input
          label="Work email"
          type="email"
          leftIcon={<Icons.mail size={17} />}
          error={errors.email?.message}
          required
          {...register('email')}
        />
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
          leftIcon={<Icons.lock size={17} />}
          hint="At least 8 characters, with upper and lower case, a digit and a symbol."
          error={errors.newPassword?.message}
          required
          {...register('newPassword')}
        />
        <Input
          label="Confirm new password"
          type="password"
          leftIcon={<Icons.lock size={17} />}
          error={errors.confirmPassword?.message}
          required
          {...register('confirmPassword')}
        />
        <Button type="submit" fullWidth size="lg" loading={isSubmitting}>
          Update password
        </Button>
      </form>
      <p className="mt-7 text-center text-sm">
        <Link to="/login" className="font-semibold text-brand-700 transition-colors hover:text-brand-800">
          Back to sign in
        </Link>
      </p>
    </Card>
  );
}
