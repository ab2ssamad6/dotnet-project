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
        title={`Hi${firstName ? `, ${firstName}` : ''}`}
        description="Track your learning and pick up where you left off."
        actions={
          <Link to="/catalog">
            <Button leftIcon={<Icons.grid size={18} />}>Browse catalog</Button>
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
            <StatCard label="Enrolled" value={enrollments.length} icon={<Icons.book size={24} />} accent="brand" to="/my-learning" />
            <StatCard label="In progress" value={active.length} icon={<Icons.play size={24} />} accent="sky" />
            <StatCard label="Completed" value={completed.length} icon={<Icons.award size={24} />} accent="emerald" />
            <StatCard label="Avg. progress" value={`${avgProgress}%`} icon={<Icons.dashboard size={24} />} accent="violet" />
          </div>

          <Card className="mt-6">
            <CardHeader
              title="Continue learning"
              action={
                <Link to="/my-learning" className="text-sm font-medium text-brand-600 hover:underline">
                  All courses
                </Link>
              }
            />
            <CardBody className={continueLearning.length ? '' : 'p-0'}>
              {continueLearning.length > 0 ? (
                <div className="space-y-4">
                  {continueLearning.map((e) => (
                    <div key={e.id} className="flex items-center gap-4">
                      <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-lg bg-brand-50 text-brand-600">
                        <Icons.book size={22} />
                      </span>
                      <div className="min-w-0 flex-1">
                        <div className="flex items-center justify-between gap-2">
                          <p className="truncate font-medium text-slate-800">{e.trainingTitle}</p>
                          <EnrollmentStatusBadge value={e.status} />
                        </div>
                        <div className="mt-1.5">
                          <ProgressBar value={e.progressPercent} showLabel />
                        </div>
                      </div>
                      <Link to={`/my-learning/${e.trainingId}`}>
                        <Button size="sm" variant="outline">
                          Resume
                        </Button>
                      </Link>
                    </div>
                  ))}
                </div>
              ) : (
                <EmptyState
                  icon={<Icons.book size={26} />}
                  title="No active courses"
                  description="Browse the catalog and enroll to start learning."
                  action={
                    <Link to="/catalog">
                      <Button>Browse catalog</Button>
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
