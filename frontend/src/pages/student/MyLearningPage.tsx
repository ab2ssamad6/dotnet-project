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
        title="My Learning"
        description="Your enrolled trainings and progress."
        actions={
          <Link to="/catalog">
            <Button variant="outline" leftIcon={<Icons.grid size={18} />}>
              Browse catalog
            </Button>
          </Link>
        }
      />

      <Tabs
        className="mb-5"
        active={tab}
        onChange={setTab}
        tabs={[
          { id: 'all', label: 'All', count: enrollments.length },
          { id: 'active', label: 'In progress', count: enrollments.filter((e) => e.status === EnrollmentStatus.Active).length },
          { id: 'completed', label: 'Completed', count: enrollments.filter((e) => e.status === EnrollmentStatus.Completed).length },
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
        <EmptyState
          icon={<Icons.book size={26} />}
          title="Nothing here yet"
          description="Enroll in a training from the catalog to start learning."
          action={
            <Link to="/catalog">
              <Button>Browse catalog</Button>
            </Link>
          }
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {byTab.map((e) => (
            <Card key={e.id}>
              <CardBody>
                <div className="flex items-start justify-between gap-3">
                  <div className="flex items-center gap-3">
                    <span className="flex h-11 w-11 items-center justify-center rounded-lg bg-brand-50 text-brand-600">
                      <Icons.book size={22} />
                    </span>
                    <div>
                      <h3 className="font-semibold text-slate-900">{e.trainingTitle}</h3>
                      <p className="text-xs text-slate-400">Enrolled {formatDate(e.enrolledAt)}</p>
                    </div>
                  </div>
                  <EnrollmentStatusBadge value={e.status} />
                </div>
                <div className="mt-4">
                  <div className="mb-1 flex justify-between text-xs text-slate-500">
                    <span>Progress</span>
                    <span className="font-medium">{e.progressPercent}%</span>
                  </div>
                  <ProgressBar value={e.progressPercent} />
                </div>
                <div className="mt-4">
                  <Link to={`/my-learning/${e.trainingId}`}>
                    <Button fullWidth variant={e.progressPercent > 0 ? 'primary' : 'outline'}>
                      {e.status === EnrollmentStatus.Completed
                        ? 'Review course'
                        : e.progressPercent > 0
                          ? 'Continue'
                          : 'Start learning'}
                    </Button>
                  </Link>
                </div>
              </CardBody>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
