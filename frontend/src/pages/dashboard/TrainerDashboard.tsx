import { Link } from 'react-router-dom';
import { Button, Card, CardBody, CardHeader, Icons, SkeletonCard } from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { StatCard } from '@/components/common/StatCard';
import { DifficultyBadge, TrainingStatusBadge } from '@/components/common/StatusBadge';
import { useAsync } from '@/hooks/useAsync';
import { categoryService, trainerService, trainingService } from '@/services';
import { formatDuration } from '@/utils/format';

export function TrainerDashboard({ firstName }: { firstName?: string }) {
  const { data, loading } = useAsync(async () => {
    const [trainings, categories, trainers] = await Promise.all([
      trainingService.list({ page: 1, pageSize: 5 }),
      categoryService.list({ page: 1, pageSize: 1 }),
      trainerService.list({ page: 1, pageSize: 1 }),
    ]);
    return { trainings, categories, trainers };
  }, []);

  const published = data?.trainings.items.filter((t) => t.published).length ?? 0;

  return (
    <div>
      <PageHeader
        eyebrow="Trainer"
        title={`Welcome back${firstName ? `, ${firstName}` : ''}`}
        description="Your content at a glance — what's live, what's still in draft, and what to work on next."
        actions={
          <Link to="/trainings">
            <Button leftIcon={<Icons.plus size={17} />}>New training</Button>
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
            <StatCard label="Trainings" value={data?.trainings.totalCount ?? 0} icon={<Icons.training size={19} />} accent="sky" to="/trainings" />
            <StatCard label="Published" value={published} icon={<Icons.check size={19} />} accent="green" hint="Visible in the catalog" />
            <StatCard label="Categories" value={data?.categories.totalCount ?? 0} icon={<Icons.category size={19} />} accent="amber" to="/categories" />
            <StatCard label="Trainers" value={data?.trainers.totalCount ?? 0} icon={<Icons.trainer size={19} />} accent="violet" to="/trainers" />
          </div>

          <Card className="mt-6">
            <CardHeader
              title="Recent trainings"
              subtitle="Your most recently updated courses"
              action={
                <Link
                  to="/trainings"
                  className="inline-flex items-center gap-1 text-[13px] font-semibold text-brand-700 transition-colors hover:text-brand-800"
                >
                  View all <Icons.arrowRight size={14} />
                </Link>
              }
            />
            <CardBody className="p-0">
              {data && data.trainings.items.length > 0 ? (
                <ul className="divide-y divide-ink-100">
                  {data.trainings.items.map((t) => (
                    <li key={t.id}>
                      <Link
                        to={`/trainings/${t.id}`}
                        className="group flex items-center gap-3.5 px-5 py-4 transition-colors hover:bg-ink-50/70"
                      >
                        <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-200/60">
                          <Icons.training size={18} />
                        </span>
                        <div className="min-w-0 flex-1">
                          <p className="truncate text-sm font-bold text-ink-800 group-hover:text-brand-800">{t.title}</p>
                          <p className="mt-0.5 truncate text-[11.5px] font-medium text-ink-400">
                            {t.categoryName ?? 'Uncategorized'} · {formatDuration(t.duration)} · {t.moduleCount} modules
                          </p>
                        </div>
                        <div className="hidden items-center gap-2 sm:flex">
                          <DifficultyBadge value={t.difficulty} />
                          <TrainingStatusBadge value={t.status} />
                        </div>
                        <Icons.chevronRight size={16} className="shrink-0 text-ink-300 group-hover:text-ink-500" />
                      </Link>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="px-5 py-12 text-center text-sm text-ink-400">
                  No trainings yet — create your first course to get started.
                </p>
              )}
            </CardBody>
          </Card>
        </>
      )}
    </div>
  );
}
