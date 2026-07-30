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
      <Card className="p-8 text-center shadow-raised sm:p-9">
        <div className="relative mx-auto mb-5 w-fit">
          <span className="absolute inset-0 -m-2 rounded-2xl bg-green-100/70 blur-md" aria-hidden />
          <span className="relative flex h-14 w-14 items-center justify-center rounded-2xl border border-green-200/70 bg-white text-green-600 shadow-card">
            <Icons.mail size={24} />
          </span>
        </div>
        <h1 className="font-display text-[23px] font-semibold tracking-[-0.02em] text-ink-900">Check your inbox</h1>
        <p className="mx-auto mt-3 max-w-sm text-sm leading-relaxed text-ink-500">
          If an account exists for <span className="font-semibold text-ink-800">{getValues('email')}</span>, a
          reset token is on its way. Paste it on the next screen to choose a new password.
        </p>
        <div className="mt-7 flex flex-col gap-2.5">
          <Link to="/reset-password">
            <Button fullWidth rightIcon={<Icons.arrowRight size={16} />}>
              Enter reset token
            </Button>
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
    <Card className="p-8 shadow-raised sm:p-9">
      <p className="eyebrow">Account recovery</p>
      <h1 className="mt-2 font-display text-[27px] font-semibold tracking-[-0.02em] text-ink-900">
        Reset your password
      </h1>
      <p className="mt-2 text-sm leading-relaxed text-ink-500">
        Tell us the email on your account and we'll send a single-use token to set a new password.
      </p>
      <form onSubmit={handleSubmit(onSubmit)} className="mt-7 space-y-4">
        <Input
          label="Work email"
          type="email"
          placeholder="you@company.com"
          leftIcon={<Icons.mail size={17} />}
          error={errors.email?.message}
          required
          {...register('email')}
        />
        <Button type="submit" fullWidth size="lg" loading={isSubmitting}>
          Send reset token
        </Button>
      </form>
      <p className="mt-7 text-center text-sm text-ink-500">
        Remembered it?{' '}
        <Link to="/login" className="font-semibold text-brand-700 transition-colors hover:text-brand-800">
          Sign in
        </Link>
      </p>
    </Card>
  );
}
