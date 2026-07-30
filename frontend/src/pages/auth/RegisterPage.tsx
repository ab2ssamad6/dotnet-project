import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { Button, Card, Icons, Input, Select } from '@/components/ui';
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
      notify.success(`Account ready. Welcome aboard, ${user.firstName}.`);
      navigate('/dashboard', { replace: true });
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <Card className="p-8 shadow-raised sm:p-9">
      <p className="eyebrow">Get started</p>
      <h1 className="mt-2 font-display text-[27px] font-semibold tracking-[-0.02em] text-ink-900">
        Create your account
      </h1>
      <p className="mt-2 text-sm text-ink-500">Join as a learner, or as a trainer who builds the courses.</p>

      <form onSubmit={handleSubmit(onSubmit)} className="mt-7 space-y-4">
        <div className="grid grid-cols-2 gap-3">
          <Input label="First name" placeholder="Jane" error={errors.firstName?.message} required {...register('firstName')} />
          <Input label="Last name" placeholder="Doe" error={errors.lastName?.message} required {...register('lastName')} />
        </div>
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
        <Select
          label="How will you use the platform?"
          error={errors.role?.message}
          required
          options={[
            { value: Role.Student, label: 'Learner — enroll in trainings and track progress' },
            { value: Role.Trainer, label: 'Trainer — build and publish course content' },
          ]}
          {...register('role')}
        />
        <Input
          label="Password"
          type="password"
          autoComplete="new-password"
          placeholder="••••••••"
          leftIcon={<Icons.lock size={17} />}
          hint="At least 8 characters, with upper and lower case, a digit and a symbol."
          error={errors.password?.message}
          required
          {...register('password')}
        />
        <Input
          label="Confirm password"
          type="password"
          autoComplete="new-password"
          placeholder="••••••••"
          leftIcon={<Icons.lock size={17} />}
          error={errors.confirmPassword?.message}
          required
          {...register('confirmPassword')}
        />

        <Button type="submit" fullWidth size="lg" loading={isSubmitting} rightIcon={<Icons.arrowRight size={17} />}>
          Create account
        </Button>
      </form>

      <p className="mt-7 text-center text-sm text-ink-500">
        Already registered?{' '}
        <Link to="/login" className="font-semibold text-brand-700 transition-colors hover:text-brand-800">
          Sign in instead
        </Link>
      </p>
    </Card>
  );
}
