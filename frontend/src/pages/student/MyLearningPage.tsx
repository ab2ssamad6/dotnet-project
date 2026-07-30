import { Link } from 'react-router-dom';
import { Button, Card, CardBody, Icons, ProgressBar, SkeletonCard, EmptyState, Tabs } from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { ErrorState } from '@/components/common/ErrorState';
import { EnrollmentStatusBadge } from '@/components/common/StatusBadge';
import { useAsync } from '@/hooks/useAsync';
import { enrollmentService } from '@/services';
import { formatDate } from '@/utils/format';
import { EnrollmentStatus } from '@/types';
import { useState } from 'react';

export function MyLearningPage() {
  const { data, loading, error, refetch } = useAsync(() => enrollmentService.myEnrollments(), []);
  const [tab, setTab] = useState('all');

  const enrollments = data ?? [];
  const byTab =
    tab === 'active'
      ? enrollments.filter((e) => e.status === EnrollmentStatus.Active)
      : tab === 'completed'
        ? enrollments.filter((e) => e.status === EnrollmentStatus.Completed)
        : enrollments;

  return (
    <div>
      <PageHeader
        eyebrow="Your learning"
        title="My Learning"
        description="Every course you've enrolled in, with exactly how far you've come."
        actions={
          <Link to="/catalog">
            <Button variant="outline" leftIcon={<Icons.grid size={17} />}>
              Browse catalog
            </Button>
          </Link>
        }
      />

      <Tabs
        className="mb-6"
        active={tab}
        onChange={setTab}
        tabs={[
          { id: 'all', label: 'All courses', count: enrollments.length },
          {
            id: 'active',
            label: 'In progress',
            count: enrollments.filter((e) => e.status === EnrollmentStatus.Active).length,
          },
          {
            id: 'completed',
            label: 'Completed',
            count: enrollments.filter((e) => e.status === EnrollmentStatus.Completed).length,
          },
        ]}
      />

      {error ? (
        <ErrorState error={error} onRetry={refetch} />
      ) : loading ? (
        <div className="grid gap-4 sm:grid-cols-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      ) : byTab.length === 0 ? (
        <div className="surface">
          <EmptyState
            icon={<Icons.book size={24} />}
            title={tab === 'completed' ? 'No completed courses yet' : 'Nothing here yet'}
            description="Enroll from the catalog and your courses — along with your progress — will live here."
            action={
              <Link to="/catalog">
                <Button rightIcon={<Icons.arrowRight size={16} />}>Browse catalog</Button>
              </Link>
            }
          />
        </div>
      ) : (
        <div className="grid gap-5 sm:grid-cols-2">
          {byTab.map((e) => {
            const done = e.status === EnrollmentStatus.Completed;
            return (
              <Card key={e.id} className="transition-all duration-200 hover:border-ink-300/80 hover:shadow-raised">
                <CardBody>
                  <div className="flex items-start justify-between gap-3">
                    <div className="flex min-w-0 items-center gap-3">
                      <span
                        className={`flex h-11 w-11 shrink-0 items-center justify-center rounded-xl ring-1 ring-inset ${
                          done
                            ? 'bg-green-50 text-green-700 ring-green-200/60'
                            : 'bg-brand-50 text-brand-700 ring-brand-200/60'
                        }`}
                      >
                        {done ? <Icons.award size={20} /> : <Icons.book size={20} />}
                      </span>
                      <div className="min-w-0">
                        <h3 className="truncate text-[15px] font-bold tracking-[-0.01em] text-ink-900">
                          {e.trainingTitle}
                        </h3>
                        <p className="mt-0.5 text-[11.5px] font-medium text-ink-400">
                          Enrolled {formatDate(e.enrolledAt)}
                        </p>
                      </div>
                    </div>
                    <EnrollmentStatusBadge value={e.status} />
                  </div>

                  <div className="mt-5">
                    <div className="mb-1.5 flex items-baseline justify-between text-[11.5px] font-semibold uppercase tracking-[0.08em] text-ink-400">
                      <span>Progress</span>
                      <span className="tnum text-ink-700">{e.progressPercent}%</span>
                    </div>
                    <ProgressBar value={e.progressPercent} />
                  </div>

                  <div className="mt-5">
                    <Link to={`/my-learning/${e.trainingId}`}>
                      <Button
                        fullWidth
                        variant={e.progressPercent > 0 && !done ? 'primary' : 'outline'}
                        rightIcon={<Icons.arrowRight size={15} />}
                      >
                        {done ? 'Review course' : e.progressPercent > 0 ? 'Continue where you left off' : 'Start learning'}
                      </Button>
                    </Link>
                  </div>
                </CardBody>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
