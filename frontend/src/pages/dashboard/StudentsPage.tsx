import { Card, CardBody, CardHeader, Icons, SkeletonCard } from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { StatCard } from '@/components/common/StatCard';
import { ErrorState } from '@/components/common/ErrorState';
import { useAsync } from '@/hooks/useAsync';
import { dashboardService } from '@/services';
import { timeAgo } from '@/utils/format';

export function StudentsPage() {
  const { data, loading, error, refetch } = useAsync(() => dashboardService.get(), []);
  const recentEnrollments = data?.recentActivity.filter((a) => a.type === 'Enrollment') ?? [];

  return (
    <div>
      <PageHeader title="Students" description="Student engagement across the platform." />

      {error ? (
        <ErrorState error={error} onRetry={refetch} />
      ) : loading ? (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      ) : (
        data && (
          <>
            <div className="grid grid-cols-2 gap-4 lg:grid-cols-3">
              <StatCard label="Total students" value={data.counts.students} icon={<Icons.users size={24} />} accent="brand" />
              <StatCard label="Total enrollments" value={data.counts.enrollments} icon={<Icons.enrollment size={24} />} accent="emerald" />
              <StatCard
                label="Avg. enrollments / student"
                value={data.counts.students ? (data.counts.enrollments / data.counts.students).toFixed(1) : '0'}
                icon={<Icons.dashboard size={24} />}
                accent="violet"
              />
            </div>

            <Card className="mt-6">
              <CardHeader title="Recent enrollments" subtitle="Latest student activity" />
              <CardBody className="p-0">
                {recentEnrollments.length === 0 ? (
                  <p className="px-5 py-10 text-center text-sm text-slate-400">No recent enrollments.</p>
                ) : (
                  <ul className="divide-y divide-slate-100">
                    {recentEnrollments.map((a, i) => (
                      <li key={i} className="flex items-center gap-3 px-5 py-3.5">
                        <span className="flex h-9 w-9 items-center justify-center rounded-full bg-emerald-50 text-emerald-600">
                          <Icons.enrollment size={17} />
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

            <div className="mt-4 flex items-start gap-2 rounded-lg border border-sky-100 bg-sky-50 px-4 py-3 text-sm text-sky-700">
              <Icons.info size={18} className="mt-0.5 shrink-0" />
              <p>
                Individual student records are managed through enrollments. The current API surfaces aggregate metrics
                rather than a per-student directory.
              </p>
            </div>
          </>
        )
      )}
    </div>
  );
}
