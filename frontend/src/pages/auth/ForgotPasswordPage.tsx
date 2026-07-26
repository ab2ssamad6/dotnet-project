import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link } from 'react-router-dom';
import { Button, Card, Icons, Input } from '@/components/ui';
import { zodResolver } from '@/utils/zodResolver';
import { forgotPasswordSchema, type ForgotPasswordForm } from '@/features/auth/schemas';
import { authService } from '@/services';
import { notify } from '@/utils/toast';

export function ForgotPasswordPage() {
  const [sent, setSent] = useState(false);
  const {
    register,
    handleSubmit,
    getValues,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordForm>({ resolver: zodResolver(forgotPasswordSchema) });

  const onSubmit = async (data: ForgotPasswordForm) => {
    try {
      await authService.forgotPassword(data);
      setSent(true);
    } catch (err) {
      notify.apiError(err);
    }
  };

  if (sent) {
    return (
      <Card className="p-8 text-center">
        <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-emerald-100 text-emerald-600">
          <Icons.check size={28} />
        </div>
        <h1 className="text-xl font-bold text-slate-900">Check your email</h1>
        <p className="mt-2 text-sm text-slate-500">
          If an account exists for <span className="font-medium text-slate-700">{getValues('email')}</span>, we've sent
          a password-reset token. Use it on the reset page to choose a new password.
        </p>
        <div className="mt-6 flex flex-col gap-2">
          <Link to="/reset-password">
            <Button fullWidth>Enter reset token</Button>
          </Link>
          <Link to="/login">
            <Button variant="ghost" fullWidth>
              Back to sign in
            </Button>
          </Link>
        </div>
      </Card>
    );
  }

  return (
    <Card className="p-8">
      <h1 className="text-2xl font-bold text-slate-900">Forgot password?</h1>
      <p className="mt-1 text-sm text-slate-500">
        Enter your email and we'll send you a token to reset your password.
      </p>
      <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
        <Input
          label="Email"
          type="email"
          placeholder="you@example.com"
          leftIcon={<Icons.profile size={18} />}
          error={errors.email?.message}
          required
          {...register('email')}
        />
        <Button type="submit" fullWidth size="lg" loading={isSubmitting}>
          Send reset token
        </Button>
      </form>
      <p className="mt-6 text-center text-sm text-slate-500">
        Remembered it?{' '}
        <Link to="/login" className="font-medium text-brand-600 hover:underline">
          Sign in
        </Link>
      </p>
    </Card>
  );
}
