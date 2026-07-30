import { Link } from 'react-router-dom';
import { Button, Card, CardBody, CardHeader, Icons, ProgressBar, SkeletonCard } from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { StatCard } from '@/components/common/StatCard';
import { EnrollmentStatusBadge } from '@/components/common/StatusBadge';
import { EmptyState } from '@/components/ui';
import { useAsync } from '@/hooks/useAsync';
import { enrollmentService } from '@/services';
import { EnrollmentStatus } from '@/types';

export function StudentDashboard({ firstName }: { firstName?: string }) {
  const { data, loading } = useAsync(() => enrollmentService.myEnrollments(), []);

  const enrollments = data ?? [];
  const active = enrollments.filter((e) => e.status === EnrollmentStatus.Active);
  const completed = enrollments.filter((e) => e.status === EnrollmentStatus.Completed);
  const avgProgress =
    enrollments.length > 0 ? Math.round(enrollments.reduce((s, e) => s + e.progressPercent, 0) / enrollments.length) : 0;
  const continueLearning = [...active].sort((a, b) => b.progressPercent - a.progressPercent).slice(0, 3);

  return (
    <div>
      <PageHeader
        eyebrow="Your learning"
        title={`Hello${firstName ? `, ${firstName}` : ''}`}
        description="Everything you're enrolled in, and the next step waiting for you."
        actions={
          <Link to="/catalog">
            <Button leftIcon={<Icons.grid size={17} />}>Browse catalog</Button>
          </Link>
        }
      />

      {loading ? (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      ) : (
        <>
          <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
            <StatCard label="Enrolled" value={enrollments.length} icon={<Icons.book size={19} />} accent="brand" to="/my-learning" />
            <StatCard label="In progress" value={active.length} icon={<Icons.play size={19} />} accent="sky" hint="Courses underway" />
            <StatCard label="Completed" value={completed.length} icon={<Icons.award size={19} />} accent="green" hint="Finished courses" />
            <StatCard label="Avg. progress" value={`${avgProgress}%`} icon={<Icons.trendUp size={19} />} accent="violet" hint="Across all courses" />
          </div>

          <Card className="mt-6">
            <CardHeader
              title="Continue learning"
              subtitle="Your closest finish lines first"
              action={
                <Link
                  to="/my-learning"
                  className="inline-flex items-center gap-1 text-[13px] font-semibold text-brand-700 transition-colors hover:text-brand-800"
                >
                  All courses <Icons.arrowRight size={14} />
                </Link>
              }
            />
            <CardBody className={continueLearning.length ? '' : 'p-0'}>
              {continueLearning.length > 0 ? (
                <div className="space-y-3">
                  {continueLearning.map((e) => (
                    <div
                      key={e.id}
                      className="flex items-center gap-4 rounded-xl border border-ink-200/70 bg-ink-50/40 p-3.5 transition-colors hover:border-ink-300/70 hover:bg-ink-50"
                    >
                      <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-white text-brand-700 shadow-card ring-1 ring-inset ring-ink-200/70">
                        <Icons.book size={20} />
                      </span>
                      <div className="min-w-0 flex-1">
                        <div className="flex items-center justify-between gap-2">
                          <p className="truncate text-sm font-bold text-ink-800">{e.trainingTitle}</p>
                          <EnrollmentStatusBadge value={e.status} />
                        </div>
                        <div className="mt-2">
                          <ProgressBar value={e.progressPercent} showLabel size="sm" />
                        </div>
                      </div>
                      <Link to={`/my-learning/${e.trainingId}`} className="shrink-0">
                        <Button size="sm" variant="outline" rightIcon={<Icons.arrowRight size={14} />}>
                          Resume
                        </Button>
                      </Link>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState
                  icon={<Icons.book size={24} />}
                  title="No courses in progress"
                  description="Browse the catalog and enroll — your progress will show up right here."
                  action={
                    <Link to="/catalog">
                      <Button rightIcon={<Icons.arrowRight size={16} />}>Browse catalog</Button>
                    </Link>
                  }
                />
              )}
            </CardBody>
          </Card>
        </>
      )}
    </div>
  );
}
