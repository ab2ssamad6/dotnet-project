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
        title={`Welcome${firstName ? `, ${firstName}` : ''}`}
        description="Manage your training content."
        actions={
          <Link to="/trainings">
            <Button leftIcon={<Icons.plus size={18} />}>New training</Button>
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
            <StatCard label="Trainings" value={data?.trainings.totalCount ?? 0} icon={<Icons.training size={24} />} accent="sky" to="/trainings" />
            <StatCard label="Published" value={published} icon={<Icons.check size={24} />} accent="emerald" />
            <StatCard label="Categories" value={data?.categories.totalCount ?? 0} icon={<Icons.category size={24} />} accent="amber" to="/categories" />
            <StatCard label="Trainers" value={data?.trainers.totalCount ?? 0} icon={<Icons.trainer size={24} />} accent="violet" to="/trainers" />
          </div>

          <Card className="mt-6">
            <CardHeader
              title="Recent trainings"
              action={
                <Link to="/trainings" className="text-sm font-medium text-brand-600 hover:underline">
                  View all
                </Link>
              }
            />
            <CardBody className="p-0">
              {data && data.trainings.items.length > 0 ? (
                <ul className="divide-y divide-slate-100">
                  {data.trainings.items.map((t) => (
                    <li key={t.id}>
                      <Link to={`/trainings/${t.id}`} className="flex items-center gap-3 px-5 py-3.5 hover:bg-slate-50">
                        <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-brand-50 text-brand-600">
                          <Icons.training size={20} />
                        </span>
                        <div className="min-w-0 flex-1">
                          <p className="truncate font-medium text-slate-800">{t.title}</p>
                          <p className="text-xs text-slate-400">
                            {t.categoryName ?? 'Uncategorized'} · {formatDuration(t.duration)} · {t.moduleCount} modules
                          </p>
                        </div>
                        <div className="hidden items-center gap-2 sm:flex">
                          <DifficultyBadge value={t.difficulty} />
                          <TrainingStatusBadge value={t.status} />
                        </div>
                      </Link>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="px-5 py-10 text-center text-sm text-slate-400">No trainings yet. Create your first one.</p>
              )}
            </CardBody>
          </Card>
        </>
      )}
    </div>
  );
}
