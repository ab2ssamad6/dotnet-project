import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Button,
  Icons,
  Pagination,
  SearchInput,
  Select,
  SkeletonCard,
  EmptyState,
  useConfirm,
} from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { ErrorState } from '@/components/common/ErrorState';
import { TrainingCard } from './TrainingCard';
import { TrainingFormModal } from './TrainingFormModal';
import { usePagedList } from '@/hooks/usePagedList';
import { useDisclosure } from '@/hooks/useDisclosure';
import { trainingService } from '@/services';
import { notify } from '@/utils/toast';
import { DIFFICULTY_OPTIONS, TRAINING_STATUS_OPTIONS } from '@/constants/enums';
import { TrainingStatus, type TrainingDto } from '@/types';

export function TrainingsPage() {
  const navigate = useNavigate();
  const list = usePagedList(trainingService.list, { pageSize: 9 });
  const modal = useDisclosure<TrainingDto>();
  const confirm = useConfirm();
  const [statusFilter, setStatusFilter] = useState('');
  const [difficultyFilter, setDifficultyFilter] = useState('');

  const filtered = useMemo(() => {
    return list.items.filter((t) => {
      if (statusFilter !== '' && t.status !== Number(statusFilter)) return false;
      if (difficultyFilter !== '' && t.difficulty !== Number(difficultyFilter)) return false;
      return true;
    });
  }, [list.items, statusFilter, difficultyFilter]);

  const togglePublish = async (t: TrainingDto) => {
    try {
      await trainingService.update(t.id, {
        title: t.title,
        description: t.description,
        difficulty: t.difficulty,
        duration: t.duration,
        thumbnail: t.thumbnail ?? null,
        status: t.published ? TrainingStatus.Draft : TrainingStatus.Published,
        published: !t.published,
        categoryId: t.categoryId,
        trainerId: t.trainerId,
      });
      notify.success(t.published ? 'Training moved back to draft.' : 'Training is live in the catalog.');
      list.refetch();
    } catch (err) {
      notify.apiError(err);
    }
  };

  const handleDelete = async (t: TrainingDto) => {
    const ok = await confirm({
      title: 'Delete this training?',
      message: (
        <>
          <span className="font-semibold text-ink-800">"{t.title}"</span> and every module and activity inside it will
          be removed. This can't be undone.
        </>
      ),
      confirmLabel: 'Delete training',
    });
    if (!ok) return;
    try {
      await trainingService.remove(t.id);
      notify.success('Training deleted.');
      list.refetch();
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <div>
      <PageHeader
        eyebrow="Curriculum"
        title="Trainings"
        description="Author courses, structure them into modules, and publish when they're ready for learners."
        actions={
          <Button leftIcon={<Icons.plus size={17} />} onClick={() => modal.open()}>
            New training
          </Button>
        }
      />

      <div className="surface mb-5 flex flex-col gap-3 p-3 sm:flex-row sm:items-center">
        <SearchInput
          value={list.search}
          onSearch={list.setSearch}
          placeholder="Search by title…"
          className="sm:max-w-xs sm:flex-1"
        />
        <div className="flex flex-1 items-center gap-3">
          <Select
            className="min-w-[150px]"
            placeholder="All statuses"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            options={TRAINING_STATUS_OPTIONS.map((o) => ({ value: o.value, label: o.label }))}
          />
          <Select
            className="min-w-[150px]"
            placeholder="All levels"
            value={difficultyFilter}
            onChange={(e) => setDifficultyFilter(e.target.value)}
            options={DIFFICULTY_OPTIONS.map((o) => ({ value: o.value, label: o.label }))}
          />
          <p className="tnum ml-auto hidden shrink-0 pr-1 text-[12.5px] font-semibold text-ink-400 lg:block">
            {filtered.length} shown
          </p>
        </div>
      </div>

      {list.error ? (
        <ErrorState error={list.error} onRetry={list.refetch} />
      ) : list.loading ? (
        <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      ) : filtered.length === 0 ? (
        <div className="surface">
          <EmptyState
            icon={<Icons.training size={24} />}
            title="No trainings match this view"
            description="Clear the filters above, or start a new course from scratch."
            action={<Button onClick={() => modal.open()}>New training</Button>}
          />
        </div>
      ) : (
        <>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {filtered.map((t) => (
              <TrainingCard
                key={t.id}
                training={t}
                onOpen={() => navigate(`/trainings/${t.id}`)}
                onEdit={() => modal.open(t)}
                onDelete={() => handleDelete(t)}
                onTogglePublish={() => togglePublish(t)}
              />
            ))}
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

      <TrainingFormModal open={modal.isOpen} onClose={modal.close} training={modal.payload} onSaved={list.refetch} />
    </div>
  );
}
