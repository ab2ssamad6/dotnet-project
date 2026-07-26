import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Badge,
  Button,
  Icons,
  Pagination,
  SearchInput,
  SkeletonCard,
  EmptyState,
} from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { ErrorState } from '@/components/common/ErrorState';
import { DifficultyBadge } from '@/components/common/StatusBadge';
import { usePagedList } from '@/hooks/usePagedList';
import { useAsync } from '@/hooks/useAsync';
import { enrollmentService, trainingService } from '@/services';
import { notify } from '@/utils/toast';
import { useNotifications } from '@/features/notifications/NotificationsContext';
import { formatDuration } from '@/utils/format';
import type { TrainingDto } from '@/types';

export function CatalogPage() {
  const navigate = useNavigate();
  const list = usePagedList(trainingService.catalog, { pageSize: 9 });
  const enrollments = useAsync(() => enrollmentService.myEnrollments(), []);
  const { push } = useNotifications();
  const [enrollingId, setEnrollingId] = useState<string | null>(null);

  const enrolledIds = new Set((enrollments.data ?? []).map((e) => e.trainingId));

  const enroll = async (t: TrainingDto) => {
    setEnrollingId(t.id);
    try {
      await enrollmentService.enroll(t.id);
      notify.success(`Enrolled in "${t.title}".`);
      push({ title: 'Enrolled', message: `You enrolled in "${t.title}".`, type: 'success' });
      await enrollments.refetch();
    } catch (err) {
      notify.apiError(err);
    } finally {
      setEnrollingId(null);
    }
  };

  return (
    <div>
      <PageHeader title="Training Catalog" description="Browse published trainings and enroll." />

      <div className="mb-5 max-w-sm">
        <SearchInput value={list.search} onSearch={list.setSearch} placeholder="Search catalog…" />
      </div>

      {list.error ? (
        <ErrorState error={list.error} onRetry={list.refetch} />
      ) : list.loading ? (
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      ) : list.items.length === 0 ? (
        <EmptyState icon={<Icons.grid size={26} />} title="No trainings available" description="Check back later for new courses." />
      ) : (
        <>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {list.items.map((t) => {
              const enrolled = enrolledIds.has(t.id);
              return (
                <div
                  key={t.id}
                  className="flex flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-card transition-shadow hover:shadow-md"
                >
                  <div className="relative h-32 bg-slate-100">
                    {t.thumbnail ? (
                      <img src={t.thumbnail} alt={t.title} className="h-full w-full object-cover" />
                    ) : (
                      <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-brand-500 to-brand-700 text-white">
                        <Icons.training size={34} />
                      </div>
                    )}
                    {enrolled && (
                      <div className="absolute right-3 top-3">
                        <Badge className="bg-emerald-500 text-white">
                          <Icons.check size={12} /> Enrolled
                        </Badge>
                      </div>
                    )}
                  </div>
                  <div className="flex flex-1 flex-col p-4">
                    <div className="mb-2 flex items-center gap-2">
                      <DifficultyBadge value={t.difficulty} />
                      <Badge className="bg-slate-100 text-slate-600">{t.categoryName ?? 'General'}</Badge>
                    </div>
                    <h3 className="line-clamp-1 font-semibold text-slate-900">{t.title}</h3>
                    <p className="mt-1 line-clamp-2 flex-1 text-sm text-slate-500">{t.description}</p>
                    <div className="mt-3 flex items-center gap-4 text-xs text-slate-400">
                      <span className="inline-flex items-center gap-1">
                        <Icons.clock size={14} /> {formatDuration(t.duration)}
                      </span>
                      <span className="inline-flex items-center gap-1">
                        <Icons.layers size={14} /> {t.moduleCount} modules
                      </span>
                    </div>
                    <div className="mt-4 border-t border-slate-100 pt-3">
                      {enrolled ? (
                        <Button variant="outline" fullWidth onClick={() => navigate(`/my-learning/${t.id}`)}>
                          Continue learning
                        </Button>
                      ) : (
                        <Button fullWidth loading={enrollingId === t.id} onClick={() => enroll(t)}>
                          Enroll now
                        </Button>
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
          <div className="mt-4 rounded-xl border border-slate-200 bg-white">
            <Pagination
              page={list.page}
              totalPages={list.totalPages}
              totalCount={list.totalCount}
              pageSize={list.pageSize}
              onPageChange={list.setPage}
            />
          </div>
        </>
      )}
    </div>
  );
}
