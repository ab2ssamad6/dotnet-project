import { useForm } from 'react-hook-form';
import { Button, Card, CardBody, CardHeader, Checkbox, Icons, Input } from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { zodResolver } from '@/utils/zodResolver';
import { changePasswordSchema, type ChangePasswordForm } from '@/features/auth/schemas';
import { authService } from '@/services';
import { notify } from '@/utils/toast';
import { useAuth } from '@/hooks/useAuth';

export function SettingsPage() {
  const { user } = useAuth();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordForm>({ resolver: zodResolver(changePasswordSchema) });

  const onSubmit = async (data: ChangePasswordForm) => {
    try {
      await authService.changePassword(data);
      notify.success('Password changed successfully.');
      reset();
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <div>
      <PageHeader title="Settings" description="Manage your account and preferences." />

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader title="Change password" subtitle="Update the password you use to sign in." />
          <CardBody>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <Input
                label="Current password"
                type="password"
                autoComplete="current-password"
                error={errors.currentPassword?.message}
                required
                {...register('currentPassword')}
              />
              <Input
                label="New password"
                type="password"
                autoComplete="new-password"
                hint="8+ chars with upper, lower, digit & symbol."
                error={errors.newPassword?.message}
                required
                {...register('newPassword')}
              />
              <Input
                label="Confirm new password"
                type="password"
                autoComplete="new-password"
                error={errors.confirmPassword?.message}
                required
                {...register('confirmPassword')}
              />
              <Button type="submit" loading={isSubmitting} leftIcon={<Icons.check size={17} />}>
                Update password
              </Button>
            </form>
          </CardBody>
        </Card>

        <div className="space-y-6">
          <Card>
            <CardHeader title="Preferences" subtitle="Local UI preferences (stored in this browser)." />
            <CardBody className="space-y-4">
              <Checkbox
                label="Email notifications"
                description="Receive product updates by email."
                defaultChecked
              />
              <Checkbox label="In-app notifications" description="Show enrollment and assessment alerts." defaultChecked />
              <Checkbox label="Compact tables" description="Show denser data tables." />
            </CardBody>
          </Card>

          <Card>
            <CardHeader title="Account" />
            <CardBody>
              <dl className="space-y-3 text-sm">
                <div className="flex justify-between">
                  <dt className="text-slate-500">Signed in as</dt>
                  <dd className="font-medium text-slate-800">{user?.email}</dd>
                </div>
                <div className="flex justify-between">
                  <dt className="text-slate-500">Email verified</dt>
                  <dd className="font-medium text-slate-800">{user?.emailConfirmed ? 'Yes' : 'No'}</dd>
                </div>
              </dl>
            </CardBody>
          </Card>
        </div>
      </div>
    </div>
  );
}
