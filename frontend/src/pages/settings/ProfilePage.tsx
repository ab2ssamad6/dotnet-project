import { Avatar, Badge, Card, CardBody, CardHeader, Icons } from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { useAuth } from '@/hooks/useAuth';
import { fullName } from '@/utils/format';

export function ProfilePage() {
  const { user, roles } = useAuth();
  if (!user) return null;

  return (
    <div>
      <PageHeader eyebrow="Account" title="My profile" description="How you appear across the workspace." />

      <div className="grid gap-5 lg:grid-cols-3">
        <Card className="overflow-hidden lg:col-span-1">
          <div className="h-24 bg-brand-gradient">
            <span className="block h-full w-full bg-grain opacity-[0.07] mix-blend-overlay" aria-hidden />
          </div>
          <CardBody className="-mt-10 flex flex-col items-center text-center">
            <Avatar firstName={user.firstName} lastName={user.lastName} size="lg" className="ring-4 ring-white" />
            <h2 className="mt-3.5 text-lg font-bold tracking-[-0.01em] text-ink-900">
              {fullName(user.firstName, user.lastName)}
            </h2>
            <p className="mt-0.5 text-[13px] text-ink-500">{user.email}</p>
            <div className="mt-3.5 flex flex-wrap justify-center gap-1.5">
              {roles.map((r) => (
                <Badge key={r} tone="brand">
                  {r}
                </Badge>
              ))}
            </div>
            <div className="mt-5 w-full border-t border-ink-100 pt-4">
              {user.emailConfirmed ? (
                <span className="inline-flex items-center gap-2 text-[13px] font-bold text-green-600">
                  <Icons.shield size={16} /> Email verified
                </span>
              ) : (
                <span className="inline-flex items-center gap-2 text-[13px] font-bold text-amber-600">
                  <Icons.alert size={16} /> Email not verified
                </span>
              )}
            </div>
          </CardBody>
        </Card>

        <Card className="lg:col-span-2">
          <CardHeader title="Account information" subtitle="Details pulled from your identity record" />
          <CardBody>
            <dl className="grid gap-x-6 gap-y-5 sm:grid-cols-2">
              <Field label="First name" value={user.firstName} />
              <Field label="Last name" value={user.lastName} />
              <Field label="Email" value={user.email} />
              <Field label="Roles" value={roles.join(', ')} />
              <Field label="User ID" value={user.id} mono />
              <Field label="Email status" value={user.emailConfirmed ? 'Verified' : 'Pending verification'} />
            </dl>
          </CardBody>
        </Card>
      </div>
    </div>
  );
}

function Field({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className="min-w-0">
      <dt className="eyebrow">{label}</dt>
      <dd className={`mt-1.5 text-sm font-medium text-ink-800 ${mono ? 'break-all font-mono text-xs' : ''}`}>
        {value || '—'}
      </dd>
    </div>
  );
}
