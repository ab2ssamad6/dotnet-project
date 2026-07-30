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
        eyebrow="Administrator"
        title={`Good to see you${firstName ? `, ${firstName}` : ''}`}
        description="A live read on your catalog, your people and everything happening across the platform."
        actions={
          <Link to="/trainings">
            <Button leftIcon={<Icons.plus size={17} />}>New training</Button>
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
              <StatCard label="Students" value={data.counts.students} icon={<Icons.users size={19} />} accent="brand" to="/students" />
              <StatCard label="Trainers" value={data.counts.trainers} icon={<Icons.trainer size={19} />} accent="violet" to="/trainers" />
              <StatCard label="Trainings" value={data.counts.courses} icon={<Icons.training size={19} />} accent="sky" to="/trainings" />
              <StatCard label="Modules" value={data.counts.modules} icon={<Icons.layers size={19} />} accent="amber" hint="Across every course" />
              <StatCard label="Enrollments" value={data.counts.enrollments} icon={<Icons.enrollment size={19} />} accent="green" hint="Seats taken to date" />
              <StatCard
                label="Published"
                value={data.counts.publishedCourses}
                icon={<Icons.check size={19} />}
                accent="brand"
                hint={`${data.counts.courses - data.counts.publishedCourses} still in draft or archived`}
              />
            </div>

            <div className="mt-6 grid gap-5 lg:grid-cols-5">
              <Card className="lg:col-span-3">
                <CardHeader title="Recent activity" subtitle="The latest enrollments and content changes" />
                <CardBody className="p-0">
                  {data.recentActivity.length === 0 ? (
                    <p className="px-5 py-10 text-center text-sm text-ink-400">
                      Nothing has happened yet — activity shows up here as people enroll.
                    </p>
                  ) : (
                    <ul className="divide-y divide-ink-100">
                      {data.recentActivity.map((a, i) => (
                        <li key={i} className="flex items-center gap-3.5 px-5 py-4">
                          <span
                            className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset ${
                              a.type === 'Enrollment'
                                ? 'bg-green-50 text-green-600 ring-green-200/60'
                                : 'bg-sky-50 text-sky-600 ring-sky-200/60'
                            }`}
                          >
                            {a.type === 'Enrollment' ? <Icons.enrollment size={16} /> : <Icons.training size={16} />}
                          </span>
                          <div className="min-w-0 flex-1">
                            <p className="truncate text-[13.5px] font-medium text-ink-700">{a.description}</p>
                            <p className="mt-0.5 text-[11.5px] font-medium text-ink-400">{timeAgo(a.timestamp)}</p>
                          </div>
                        </li>
                      ))}
                    </ul>
                  )}
                </CardBody>
              </Card>

              <Card className="lg:col-span-2">
                <CardHeader title="Catalog mix" subtitle="Where your trainings sit by category" />
                <CardBody>
                  {data.trainingsByCategory.length === 0 ? (
                    <p className="py-8 text-center text-sm text-ink-400">No categories yet.</p>
                  ) : (
                    <ul className="space-y-4">
                      {data.trainingsByCategory.map((c) => {
                        const max = Math.max(...data.trainingsByCategory.map((x) => x.trainings), 1);
                        return (
                          <li key={c.category}>
                            <div className="mb-1.5 flex items-baseline justify-between gap-3 text-[13px]">
                              <span className="truncate font-semibold text-ink-700">{c.category}</span>
                              <span className="tnum shrink-0 font-bold text-ink-400">{c.trainings}</span>
                            </div>
                            <div className="h-2 overflow-hidden rounded-full bg-ink-100">
                              <div
                                className="h-full rounded-full bg-brand-600 transition-[width] duration-700 ease-out"
                                style={{ width: `${(c.trainings / max) * 100}%` }}
                              />
                            </div>
                          </li>
                        );
                      })}
                    </ul>
                  )}
                </CardBody>
              </Card>
            </div>

            <div className="mt-8">
              <h2 className="eyebrow mb-3">Jump back in</h2>
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                <QuickAction to="/trainings" icon={<Icons.training size={18} />} label="Trainings" hint="Build courses" />
                <QuickAction to="/categories" icon={<Icons.category size={18} />} label="Categories" hint="Organize" />
                <QuickAction to="/trainers" icon={<Icons.trainer size={18} />} label="Trainers" hint="Instructors" />
                <QuickAction to="/ai-trainer" icon={<Icons.sparkle size={18} />} label="AI Trainer" hint="Live tutor" />
              </div>
            </div>
          </>
        )
      )}
    </div>
  );
}

function QuickAction({ to, icon, label, hint }: { to: string; icon: React.ReactNode; label: string; hint: string }) {
  return (
    <Link
      to={to}
      className="focus-ring surface group flex items-center gap-3 p-4 transition-all duration-200 hover:-translate-y-0.5 hover:border-ink-300/80 hover:shadow-raised"
    >
      <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-200/60 transition-colors group-hover:bg-brand-100">
        {icon}
      </span>
      <span className="min-w-0">
        <span className="block truncate text-[13.5px] font-bold text-ink-800">{label}</span>
        <span className="block truncate text-[11.5px] text-ink-400">{hint}</span>
      </span>
    </Link>
  );
}
