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
      <PageHeader
        eyebrow="People"
        title="Students"
        description="How your learners are engaging with the catalog, measured across every enrollment."
      />

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
              <StatCard label="Total students" value={data.counts.students} icon={<Icons.users size={19} />} accent="brand" hint="Registered learners" />
              <StatCard label="Total enrollments" value={data.counts.enrollments} icon={<Icons.enrollment size={19} />} accent="green" hint="Seats taken to date" />
              <StatCard
                label="Courses per student"
                value={data.counts.students ? (data.counts.enrollments / data.counts.students).toFixed(1) : '0'}
                icon={<Icons.trendUp size={19} />}
                accent="violet"
                hint="Average enrollments each"
              />
            </div>

            <Card className="mt-6">
              <CardHeader title="Recent enrollments" subtitle="The newest learners to join a course" />
              <CardBody className="p-0">
                {recentEnrollments.length === 0 ? (
                  <p className="px-5 py-12 text-center text-sm text-ink-400">
                    No enrollments yet — they'll appear here as soon as learners sign up for a course.
                  </p>
                ) : (
                  <ul className="divide-y divide-ink-100">
                    {recentEnrollments.map((a, i) => (
                      <li key={i} className="flex items-center gap-3.5 px-5 py-4">
                        <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-green-50 text-green-600 ring-1 ring-inset ring-green-200/60">
                          <Icons.enrollment size={16} />
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

            <div className="mt-5 flex items-start gap-3 rounded-xl border border-sky-200/70 bg-sky-50/70 px-4 py-3.5 text-[13px] leading-relaxed text-sky-800">
              <Icons.info size={17} className="mt-0.5 shrink-0" />
              <p>
                Learners are managed through their enrollments. The API exposes aggregate engagement metrics here rather
                than a per-student directory.
              </p>
            </div>
          </>
        )
      )}
    </div>
  );
}
