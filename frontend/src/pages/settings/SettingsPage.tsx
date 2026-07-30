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
      notify.success('Password updated.');
      reset();
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <div>
      <PageHeader
        eyebrow="Workspace"
        title="Settings"
        description="Security and interface preferences for your account."
      />

      <div className="grid gap-5 lg:grid-cols-2">
        <Card>
          <CardHeader title="Password" subtitle="Change the password you use to sign in." />
          <CardBody>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <Input
                label="Current password"
                type="password"
                autoComplete="current-password"
                leftIcon={<Icons.lock size={17} />}
                error={errors.currentPassword?.message}
                required
                {...register('currentPassword')}
              />
              <Input
                label="New password"
                type="password"
                autoComplete="new-password"
                leftIcon={<Icons.lock size={17} />}
                hint="At least 8 characters, with upper and lower case, a digit and a symbol."
                error={errors.newPassword?.message}
                required
                {...register('newPassword')}
              />
              <Input
                label="Confirm new password"
                type="password"
                autoComplete="new-password"
                leftIcon={<Icons.lock size={17} />}
                error={errors.confirmPassword?.message}
                required
                {...register('confirmPassword')}
              />
              <Button type="submit" loading={isSubmitting} leftIcon={<Icons.shield size={16} />}>
                Update password
              </Button>
            </form>
          </CardBody>
        </Card>

        <div className="space-y-5">
          <Card>
            <CardHeader title="Preferences" subtitle="Interface options, remembered on this device." />
            <CardBody className="space-y-4">
              <Checkbox label="Email notifications" description="Product updates and course announcements." defaultChecked />
              <Checkbox
                label="In-app notifications"
                description="Enrollment and assessment alerts in the bell menu."
                defaultChecked
              />
              <Checkbox label="Compact tables" description="Tighter row spacing in data tables." />
            </CardBody>
          </Card>

          <Card>
            <CardHeader title="Account" subtitle="Where you're signed in right now" />
            <CardBody>
              <dl className="space-y-3.5 text-sm">
                <div className="flex items-center justify-between gap-4">
                  <dt className="text-ink-500">Signed in as</dt>
                  <dd className="truncate font-semibold text-ink-800">{user?.email}</dd>
                </div>
                <div className="flex items-center justify-between gap-4">
                  <dt className="text-ink-500">Email verified</dt>
                  <dd
                    className={`font-semibold ${user?.emailConfirmed ? 'text-green-600' : 'text-amber-600'}`}
                  >
                    {user?.emailConfirmed ? 'Yes' : 'Not yet'}
                  </dd>
                </div>
              </dl>
            </CardBody>
          </Card>
        </div>
      </div>
    </div>
  );
}
