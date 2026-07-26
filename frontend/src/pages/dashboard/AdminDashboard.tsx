import { Link } from 'react-router-dom';
import { Button, Card, CardBody, CardHeader, Icons, SkeletonCard } from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { StatCard } from '@/components/common/StatCard';
import { ErrorState } from '@/components/common/ErrorState';
import { useAsync } from '@/hooks/useAsync';
import { dashboardService } from '@/services';
import { timeAgo } from '@/utils/format';

export function AdminDashboard({ firstName }: { firstName?: string }) {
  const { data, loading, error, refetch } = useAsync(() => dashboardService.get(), []);

  return (
    <div>
      <PageHeader
        title={`Welcome back${firstName ? `, ${firstName}` : ''}`}
        description="Platform overview and recent activity."
        actions={
          <Link to="/trainings">
            <Button leftIcon={<Icons.plus size={18} />}>New training</Button>
          </Link>
        }
      />

      {error ? (
        <ErrorState error={error} onRetry={refetch} />
      ) : loading ? (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      ) : (
        data && (
          <>
            <div className="grid grid-cols-2 gap-4 lg:grid-cols-3">
              <StatCard label="Students" value={data.counts.students} icon={<Icons.users size={24} />} accent="brand" to="/students" />
              <StatCard label="Trainers" value={data.counts.trainers} icon={<Icons.trainer size={24} />} accent="violet" to="/trainers" />
              <StatCard label="Trainings" value={data.counts.courses} icon={<Icons.training size={24} />} accent="sky" to="/trainings" />
              <StatCard label="Modules" value={data.counts.modules} icon={<Icons.layers size={24} />} accent="amber" />
              <StatCard label="Enrollments" value={data.counts.enrollments} icon={<Icons.enrollment size={24} />} accent="emerald" />
              <StatCard
                label="Published"
                value={data.counts.publishedCourses}
                icon={<Icons.check size={24} />}
                accent="brand"
                hint={`${data.counts.courses - data.counts.publishedCourses} draft/archived`}
              />
            </div>

            <div className="mt-6 grid gap-6 lg:grid-cols-5">
              <Card className="lg:col-span-3">
                <CardHeader title="Recent activity" subtitle="Latest enrollments and content changes" />
                <CardBody className="p-0">
                  {data.recentActivity.length === 0 ? (
                    <p className="px-5 py-8 text-center text-sm text-slate-400">No recent activity.</p>
                  ) : (
                    <ul className="divide-y divide-slate-100">
                      {data.recentActivity.map((a, i) => (
                        <li key={i} className="flex items-center gap-3 px-5 py-3.5">
                          <span
                            className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-full ${
                              a.type === 'Enrollment' ? 'bg-emerald-50 text-emerald-600' : 'bg-sky-50 text-sky-600'
                            }`}
                          >
                            {a.type === 'Enrollment' ? <Icons.enrollment size={17} /> : <Icons.training size={17} />}
                          </span>
                          <div className="min-w-0 flex-1">
                            <p className="truncate text-sm text-slate-700">{a.description}</p>
                            <p className="text-xs text-slate-400">{timeAgo(a.timestamp)}</p>
                          </div>
                        </li>
                      ))}
                    </ul>
                  )}
                </CardBody>
              </Card>

              <Card className="lg:col-span-2">
                <CardHeader title="Trainings by category" subtitle="Top categories" />
                <CardBody>
                  {data.trainingsByCategory.length === 0 ? (
                    <p className="py-6 text-center text-sm text-slate-400">No categories yet.</p>
                  ) : (
                    <ul className="space-y-3">
                      {data.trainingsByCategory.map((c) => {
                        const max = Math.max(...data.trainingsByCategory.map((x) => x.trainings), 1);
                        return (
                          <li key={c.category}>
                            <div className="mb-1 flex justify-between text-sm">
                              <span className="truncate font-medium text-slate-600">{c.category}</span>
                              <span className="text-slate-400">{c.trainings}</span>
                            </div>
                            <div className="h-2 overflow-hidden rounded-full bg-slate-100">
                              <div className="h-full rounded-full bg-brand-500" style={{ width: `${(c.trainings / max) * 100}%` }} />
                            </div>
                          </li>
                        );
                      })}
                    </ul>
                  )}
                </CardBody>
              </Card>
            </div>

            <div className="mt-6">
              <h2 className="mb-3 text-sm font-semibold text-slate-700">Quick actions</h2>
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                <QuickAction to="/trainings" icon={<Icons.training size={20} />} label="Trainings" />
                <QuickAction to="/categories" icon={<Icons.category size={20} />} label="Categories" />
                <QuickAction to="/trainers" icon={<Icons.trainer size={20} />} label="Trainers" />
                <QuickAction to="/ai-trainer" icon={<Icons.ai size={20} />} label="AI Trainer" />
              </div>
            </div>
          </>
        )
      )}
    </div>
  );
}

function QuickAction({ to, icon, label }: { to: string; icon: React.ReactNode; label: string }) {
  return (
    <Link
      to={to}
      className="flex flex-col items-center gap-2 rounded-xl border border-slate-200 bg-white p-4 text-center shadow-card transition-all hover:border-brand-300 hover:shadow-md"
    >
      <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-brand-50 text-brand-600">{icon}</span>
      <span className="text-sm font-medium text-slate-700">{label}</span>
    </Link>
  );
}
