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
      notify.success(`Welcome back, ${user.firstName}!`);
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
    <Card className="p-8">
      <h1 className="text-2xl font-bold text-slate-900">Welcome back</h1>
      <p className="mt-1 text-sm text-slate-500">Sign in to your account to continue.</p>

      <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
        <Input
          label="Email"
          type="email"
          autoComplete="email"
          placeholder="you@example.com"
          leftIcon={<Icons.profile size={18} />}
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
            error={errors.password?.message}
            required
            {...register('password')}
          />
          <div className="mt-1.5 text-right">
            <Link to="/forgot-password" className="text-sm font-medium text-brand-600 hover:underline">
              Forgot password?
            </Link>
          </div>
        </div>

        <Button type="submit" fullWidth size="lg" loading={isSubmitting}>
          Sign in
        </Button>
      </form>

      <div className="mt-6 rounded-lg border border-dashed border-slate-200 bg-slate-50 p-3">
        <p className="mb-2 text-xs font-medium text-slate-500">Demo accounts (seeded):</p>
        <div className="grid gap-1.5 text-xs">
          <button
            type="button"
            onClick={() => fillDemo('admin@lms.local', 'Admin#12345')}
            className="flex justify-between rounded px-2 py-1 text-left hover:bg-white"
          >
            <span className="font-medium text-slate-600">Administrator</span>
            <span className="text-slate-400">admin@lms.local</span>
          </button>
          <button
            type="button"
            onClick={() => fillDemo('trainer@lms.local', 'Trainer#12345')}
            className="flex justify-between rounded px-2 py-1 text-left hover:bg-white"
          >
            <span className="font-medium text-slate-600">Trainer</span>
            <span className="text-slate-400">trainer@lms.local</span>
          </button>
          <button
            type="button"
            onClick={() => fillDemo('student@lms.local', 'Student#12345')}
            className="flex justify-between rounded px-2 py-1 text-left hover:bg-white"
          >
            <span className="font-medium text-slate-600">Student</span>
            <span className="text-slate-400">student@lms.local</span>
          </button>
        </div>
      </div>

      <p className="mt-6 text-center text-sm text-slate-500">
        Don't have an account?{' '}
        <Link to="/register" className="font-medium text-brand-600 hover:underline">
          Create one
        </Link>
      </p>
    </Card>
  );
}
