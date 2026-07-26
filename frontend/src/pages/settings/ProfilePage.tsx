import { Avatar, Badge, Card, CardBody, CardHeader, Icons } from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { useAuth } from '@/hooks/useAuth';
import { fullName } from '@/utils/format';

export function ProfilePage() {
  const { user, roles } = useAuth();
  if (!user) return null;

  return (
    <div>
      <PageHeader title="My Profile" description="Your account details." />

      <div className="grid gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-1">
          <CardBody className="flex flex-col items-center text-center">
            <Avatar firstName={user.firstName} lastName={user.lastName} size="lg" />
            <h2 className="mt-3 text-lg font-bold text-slate-900">{fullName(user.firstName, user.lastName)}</h2>
            <p className="text-sm text-slate-500">{user.email}</p>
            <div className="mt-3 flex flex-wrap justify-center gap-1.5">
              {roles.map((r) => (
                <Badge key={r} className="bg-brand-50 text-brand-700">
                  {r}
                </Badge>
              ))}
            </div>
            <div className="mt-4">
              {user.emailConfirmed ? (
                <span className="inline-flex items-center gap-1.5 text-sm font-medium text-emerald-600">
                  <Icons.check size={16} /> Email verified
                </span>
              ) : (
                <span className="inline-flex items-center gap-1.5 text-sm font-medium text-amber-600">
                  <Icons.alert size={16} /> Email not verified
                </span>
              )}
            </div>
          </CardBody>
        </Card>

        <Card className="lg:col-span-2">
          <CardHeader title="Account information" />
          <CardBody>
            <dl className="grid gap-x-6 gap-y-4 sm:grid-cols-2">
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
    <div>
      <dt className="text-xs font-medium uppercase tracking-wide text-slate-400">{label}</dt>
      <dd className={`mt-0.5 text-sm text-slate-800 ${mono ? 'break-all font-mono text-xs' : ''}`}>{value || '—'}</dd>
    </div>
  );
}
