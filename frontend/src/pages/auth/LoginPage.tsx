import { useForm } from 'react-hook-form';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Button, Card, Icons, Input } from '@/components/ui';
import { zodResolver } from '@/utils/zodResolver';
import { loginSchema, type LoginForm } from '@/features/auth/schemas';
import { useAuth } from '@/hooks/useAuth';
import { notify } from '@/utils/toast';

interface LocationState {
  from?: { pathname: string };
}

const DEMO_ACCOUNTS = [
  { role: 'Administrator', email: 'admin@lms.local', password: 'Admin#12345', tint: 'text-brand-700 bg-brand-50' },
  { role: 'Trainer', email: 'trainer@lms.local', password: 'Trainer#12345', tint: 'text-violet-700 bg-violet-50' },
  { role: 'Student', email: 'student@lms.local', password: 'Student#12345', tint: 'text-sky-700 bg-sky-50' },
];

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as LocationState)?.from?.pathname ?? '/dashboard';

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<LoginForm>({ resolver: zodResolver(loginSchema) });

  const onSubmit = async (data: LoginForm) => {
    try {
      const user = await login(data);
      notify.success(`Welcome back, ${user.firstName}.`);
      navigate(from, { replace: true });
    } catch (err) {
      notify.apiError(err);
    }
  };

  const fillDemo = (email: string, password: string) => {
    setValue('email', email);
    setValue('password', password);
  };

  return (
    <Card className="p-8 shadow-raised sm:p-9">
      <p className="eyebrow">Sign in</p>
      <h1 className="mt-2 font-display text-[27px] font-semibold tracking-[-0.02em] text-ink-900">Welcome back</h1>
      <p className="mt-2 text-sm text-ink-500">Pick up your trainings right where you left them.</p>

      <form onSubmit={handleSubmit(onSubmit)} className="mt-7 space-y-4">
        <Input
          label="Work email"
          type="email"
          autoComplete="email"
          placeholder="you@company.com"
          leftIcon={<Icons.mail size={17} />}
          error={errors.email?.message}
          required
          {...register('email')}
        />
        <div>
          <Input
            label="Password"
            type="password"
            autoComplete="current-password"
            placeholder="••••••••"
            leftIcon={<Icons.lock size={17} />}
            error={errors.password?.message}
            required
            {...register('password')}
          />
          <div className="mt-2 text-right">
            <Link
              to="/forgot-password"
              className="text-[13px] font-semibold text-brand-700 transition-colors hover:text-brand-800"
            >
              Forgot your password?
            </Link>
          </div>
        </div>

        <Button type="submit" fullWidth size="lg" loading={isSubmitting} rightIcon={<Icons.arrowRight size={17} />}>
          Sign in
        </Button>
      </form>

      <div className="mt-7 rounded-xl border border-ink-200/80 bg-ink-50/70 p-4">
        <div className="mb-3 flex items-center gap-2">
          <Icons.bolt size={14} className="text-gold-600" />
          <p className="text-[12px] font-bold uppercase tracking-[0.1em] text-ink-500">Demo accounts</p>
        </div>
        <div className="grid gap-1.5">
          {DEMO_ACCOUNTS.map((account) => (
            <button
              key={account.email}
              type="button"
              onClick={() => fillDemo(account.email, account.password)}
              className="focus-ring group flex items-center justify-between gap-3 rounded-lg px-2.5 py-2 text-left transition-colors hover:bg-white"
            >
              <span className={`rounded-md px-2 py-1 text-[11.5px] font-bold ${account.tint}`}>{account.role}</span>
              <span className="truncate text-[12.5px] text-ink-500 group-hover:text-ink-700">{account.email}</span>
            </button>
          ))}
        </div>
      </div>

      <p className="mt-7 text-center text-sm text-ink-500">
        New here?{' '}
        <Link to="/register" className="font-semibold text-brand-700 transition-colors hover:text-brand-800">
          Create an account
        </Link>
      </p>
    </Card>
  );
}
