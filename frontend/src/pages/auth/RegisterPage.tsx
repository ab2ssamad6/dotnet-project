import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { Button, Card, Input, Select } from '@/components/ui';
import { zodResolver } from '@/utils/zodResolver';
import { registerSchema, type RegisterForm } from '@/features/auth/schemas';
import { useAuth } from '@/hooks/useAuth';
import { Role } from '@/types';
import { notify } from '@/utils/toast';

export function RegisterPage() {
  const { register: registerUser } = useAuth();
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
    defaultValues: { role: Role.Student },
  });

  const onSubmit = async (data: RegisterForm) => {
    try {
      const user = await registerUser({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
        confirmPassword: data.confirmPassword,
        role: data.role,
      });
      notify.success(`Account created. Welcome, ${user.firstName}!`);
      navigate('/dashboard', { replace: true });
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <Card className="p-8">
      <h1 className="text-2xl font-bold text-slate-900">Create your account</h1>
      <p className="mt-1 text-sm text-slate-500">Join the platform as a student or trainer.</p>

      <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
        <div className="grid grid-cols-2 gap-3">
          <Input label="First name" placeholder="Jane" error={errors.firstName?.message} required {...register('firstName')} />
          <Input label="Last name" placeholder="Doe" error={errors.lastName?.message} required {...register('lastName')} />
        </div>
        <Input
          label="Email"
          type="email"
          autoComplete="email"
          placeholder="you@example.com"
          error={errors.email?.message}
          required
          {...register('email')}
        />
        <Select
          label="I am a…"
          error={errors.role?.message}
          required
          options={[
            { value: Role.Student, label: 'Student — enroll and learn' },
            { value: Role.Trainer, label: 'Trainer — create and manage content' },
          ]}
          {...register('role')}
        />
        <Input
          label="Password"
          type="password"
          autoComplete="new-password"
          placeholder="••••••••"
          hint="8+ chars with upper, lower, digit & symbol."
          error={errors.password?.message}
          required
          {...register('password')}
        />
        <Input
          label="Confirm password"
          type="password"
          autoComplete="new-password"
          placeholder="••••••••"
          error={errors.confirmPassword?.message}
          required
          {...register('confirmPassword')}
        />

        <Button type="submit" fullWidth size="lg" loading={isSubmitting}>
          Create account
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-slate-500">
        Already have an account?{' '}
        <Link to="/login" className="font-medium text-brand-600 hover:underline">
          Sign in
        </Link>
      </p>
    </Card>
  );
}
