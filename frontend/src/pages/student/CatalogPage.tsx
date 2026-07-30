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
      notify.success(`You're in — "${t.title}" is now in My Learning.`);
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
      <PageHeader
        eyebrow="Discover"
        title="Training catalog"
        description="Every published course, ready when you are. Enroll and it lands in My Learning."
      />

      <div className="surface mb-5 p-3">
        <SearchInput
          value={list.search}
          onSearch={list.setSearch}
          placeholder="Search the catalog…"
          className="max-w-sm"
        />
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
        <div className="surface">
          <EmptyState
            icon={<Icons.grid size={24} />}
            title="The catalog is empty for now"
            description="No published trainings match your search. Try another term, or check back soon."
          />
        </div>
      ) : (
        <>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {list.items.map((t) => {
              const enrolled = enrolledIds.has(t.id);
              return (
                <div
                  key={t.id}
                  className="surface group flex flex-col overflow-hidden transition-all duration-200 hover:-translate-y-0.5 hover:border-ink-300/80 hover:shadow-raised"
                >
                  <div className="relative h-36 bg-ink-100">
                    {t.thumbnail ? (
                      <img
                        src={t.thumbnail}
                        alt=""
                        className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-[1.04]"
                      />
                    ) : (
                      <div className="relative flex h-full w-full items-center justify-center bg-brand-gradient text-white/90">
                        <span className="absolute inset-0 bg-grain opacity-[0.08] mix-blend-overlay" aria-hidden />
                        <Icons.training size={32} />
                      </div>
                    )}
                    <span
                      className="absolute inset-x-0 bottom-0 h-16 bg-gradient-to-t from-ink-950/50 to-transparent"
                      aria-hidden
                    />
                    <span className="absolute bottom-3 left-3 text-[11px] font-bold uppercase tracking-[0.12em] text-white/85">
                      {t.categoryName ?? 'General'}
                    </span>
                    {enrolled && (
                      <span className="absolute right-3 top-3">
                        <Badge className="bg-green-600 text-white ring-green-700/30">
                          <Icons.check size={11} /> Enrolled
                        </Badge>
                      </span>
                    )}
                  </div>
                  <div className="flex flex-1 flex-col p-4">
                    <div className="mb-2.5 flex flex-wrap items-center gap-2">
                      <DifficultyBadge value={t.difficulty} />
                      <Badge tone="neutral">
                        <Icons.clock size={11} /> {formatDuration(t.duration)}
                      </Badge>
                    </div>
                    <h3 className="line-clamp-1 text-[15px] font-bold tracking-[-0.01em] text-ink-900">{t.title}</h3>
                    <p className="mt-1.5 line-clamp-2 flex-1 text-[13px] leading-relaxed text-ink-500">
                      {t.description}
                    </p>
                    <div className="mt-3.5 flex items-center gap-4 text-[11.5px] font-medium text-ink-400">
                      <span className="inline-flex items-center gap-1.5">
                        <Icons.layers size={13} /> {t.moduleCount} modules
                      </span>
                      <span className="inline-flex min-w-0 items-center gap-1.5">
                        <Icons.trainer size={13} /> <span className="truncate">{t.trainerName ?? 'Your trainer'}</span>
                      </span>
                    </div>
                    <div className="mt-4 border-t border-ink-100 pt-3.5">
                      {enrolled ? (
                        <Button
                          variant="outline"
                          fullWidth
                          rightIcon={<Icons.arrowRight size={15} />}
                          onClick={() => navigate(`/my-learning/${t.id}`)}
                        >
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
          <div className="surface mt-5">
            <Pagination
              bare
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
