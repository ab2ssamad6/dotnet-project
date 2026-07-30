import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { AnimatePresence, motion } from 'framer-motion';
import {
  Badge,
  Button,
  Card,
  CardBody,
  Icons,
  Spinner,
  useConfirm,
} from '@/components/ui';
import { Breadcrumbs } from '@/components/common/Breadcrumbs';
import { ErrorState } from '@/components/common/ErrorState';
import { DifficultyBadge, TrainingStatusBadge } from '@/components/common/StatusBadge';
import { FullPageLoader } from '@/components/common/FullPageLoader';
import { useAsync } from '@/hooks/useAsync';
import { useDisclosure } from '@/hooks/useDisclosure';
import { moduleService, trainingService } from '@/services';
import { notify } from '@/utils/toast';
import { formatDuration } from '@/utils/format';
import type { ModuleDto } from '@/types';
import { ModuleFormModal } from './ModuleFormModal';
import { ActivitiesPanel } from './ActivitiesPanel';

export function TrainingDetailPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const confirm = useConfirm();

  const training = useAsync(() => trainingService.get(id), [id]);
  const modules = useAsync(() => moduleService.listByTraining(id), [id]);
  const moduleModal = useDisclosure<ModuleDto>();
  const [expanded, setExpanded] = useState<string | null>(null);

  const ordered = [...(modules.data ?? [])].sort((a, b) => a.order - b.order);
  const nextOrder = ordered.length ? Math.max(...ordered.map((m) => m.order)) + 1 : 1;

  const reorder = async (module: ModuleDto, direction: -1 | 1) => {
    const index = ordered.findIndex((m) => m.id === module.id);
    const swapWith = ordered[index + direction];
    if (!swapWith) return;
    try {
      await Promise.all([
        moduleService.update(module.id, { ...toUpdate(module), order: swapWith.order }),
        moduleService.update(swapWith.id, { ...toUpdate(swapWith), order: module.order }),
      ]);
      modules.refetch();
    } catch (err) {
      notify.apiError(err);
    }
  };

  const handleDeleteModule = async (module: ModuleDto) => {
    const ok = await confirm({
      title: 'Delete this module?',
      message: (
        <>
          <span className="font-semibold text-ink-800">"{module.title}"</span> and all of its activities will be
          removed from the curriculum.
        </>
      ),
      confirmLabel: 'Delete module',
    });
    if (!ok) return;
    try {
      await moduleService.remove(module.id);
      notify.success('Module deleted.');
      modules.refetch();
      training.refetch();
    } catch (err) {
      notify.apiError(err);
    }
  };

  if (training.loading) return <FullPageLoader label="Loading training…" />;
  if (training.error) return <ErrorState error={training.error} onRetry={training.refetch} />;
  const t = training.data;
  if (!t) return null;

  return (
    <div>
      <Breadcrumbs items={[{ label: 'Trainings', to: '/trainings' }, { label: t.title }]} />

      <Card className="mb-7 overflow-hidden">
        <div className="relative h-44 bg-brand-gradient">
          {t.thumbnail ? (
            <img src={t.thumbnail} alt="" className="h-full w-full object-cover opacity-45" />
          ) : (
            <span className="absolute inset-0 bg-grain opacity-[0.07] mix-blend-overlay" aria-hidden />
          )}
          <span
            className="absolute inset-0"
            style={{ background: 'radial-gradient(28rem 14rem at 10% 0%, rgb(255 255 255 / 0.16), transparent 70%)' }}
            aria-hidden
          />
          <div className="absolute inset-0 flex items-start justify-between gap-3 p-5">
            <div className="flex flex-wrap items-center gap-2">
              <TrainingStatusBadge value={t.status} />
              <DifficultyBadge value={t.difficulty} />
            </div>
            <div className="flex shrink-0 gap-2">
              <Button
                variant="outline"
                size="sm"
                leftIcon={<Icons.sparkle size={15} />}
                onClick={() => navigate(`/ai-trainer?trainingId=${id}`)}
              >
                Train with AI
              </Button>
              <Button
                variant="outline"
                size="sm"
                leftIcon={<Icons.chevronLeft size={15} />}
                onClick={() => navigate('/trainings')}
              >
                All trainings
              </Button>
            </div>
          </div>
        </div>
        <CardBody className="p-6">
          <h1 className="font-display text-[26px] font-semibold leading-tight tracking-[-0.02em] text-ink-900">
            {t.title}
          </h1>
          <p className="mt-2.5 max-w-3xl text-sm leading-relaxed text-ink-500">{t.description}</p>
          <div className="mt-5 flex flex-wrap gap-x-6 gap-y-2.5 border-t border-ink-100 pt-4 text-[13px] font-medium text-ink-600">
            <Meta icon={<Icons.category size={15} />} label={t.categoryName ?? 'Uncategorized'} />
            <Meta icon={<Icons.trainer size={15} />} label={t.trainerName ?? 'Unassigned'} />
            <Meta icon={<Icons.clock size={15} />} label={formatDuration(t.duration)} />
            <Meta icon={<Icons.layers size={15} />} label={`${ordered.length} modules`} />
          </div>
        </CardBody>
      </Card>

      <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
        <div>
          <p className="eyebrow">Curriculum</p>
          <h2 className="mt-1.5 font-display text-[21px] font-semibold tracking-[-0.02em] text-ink-900">Modules</h2>
        </div>
        <Button leftIcon={<Icons.plus size={17} />} onClick={() => moduleModal.open()}>
          Add module
        </Button>
      </div>

      {modules.loading ? (
        <div className="flex justify-center py-12 text-ink-400">
          <Spinner />
        </div>
      ) : ordered.length === 0 ? (
        <Card>
          <CardBody className="py-14 text-center">
            <span className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl border border-ink-200/70 bg-white text-brand-600 shadow-card">
              <Icons.layers size={24} />
            </span>
            <h3 className="text-base font-bold text-ink-900">Start building the curriculum</h3>
            <p className="mx-auto mt-1.5 max-w-sm text-sm leading-relaxed text-ink-500">
              Modules hold the lessons, exercises and assessments learners work through in order.
            </p>
            <Button className="mt-6" leftIcon={<Icons.plus size={17} />} onClick={() => moduleModal.open()}>
              Add the first module
            </Button>
          </CardBody>
        </Card>
      ) : (
        <div className="space-y-3">
          {ordered.map((m, i) => {
            const isOpen = expanded === m.id;
            return (
              <Card key={m.id} className={isOpen ? 'border-brand-200 shadow-raised' : undefined}>
                <div className="flex items-center gap-3 px-4 py-3.5">
                  <div className="flex flex-col text-ink-300">
                    <button
                      onClick={() => reorder(m, -1)}
                      disabled={i === 0}
                      className="focus-ring rounded transition-colors hover:text-ink-600 disabled:opacity-30"
                      aria-label="Move module up"
                    >
                      <Icons.arrowUp size={13} />
                    </button>
                    <button
                      onClick={() => reorder(m, 1)}
                      disabled={i === ordered.length - 1}
                      className="focus-ring rounded transition-colors hover:text-ink-600 disabled:opacity-30"
                      aria-label="Move module down"
                    >
                      <Icons.arrowDown size={13} />
                    </button>
                  </div>
                  <span className="tnum flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-brand-50 text-[13px] font-bold text-brand-700 ring-1 ring-inset ring-brand-200/60">
                    {m.order}
                  </span>
                  <button
                    className="focus-ring min-w-0 flex-1 rounded text-left"
                    onClick={() => setExpanded(isOpen ? null : m.id)}
                  >
                    <div className="flex items-center gap-2">
                      <p className="truncate text-sm font-bold text-ink-800">{m.title}</p>
                      {m.aiAvatarEnabled && (
                        <Badge tone="ai">
                          <Icons.sparkle size={11} /> AI
                        </Badge>
                      )}
                    </div>
                    <p className="mt-0.5 truncate text-[11.5px] font-medium text-ink-400">
                      {formatDuration(m.duration)}
                      {m.description ? ` · ${m.description}` : ''}
                    </p>
                  </button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-9 w-9"
                    onClick={() => moduleModal.open(m)}
                    aria-label="Edit module"
                  >
                    <Icons.edit size={16} />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-9 w-9 text-rose-500 hover:bg-rose-50 hover:text-rose-600"
                    onClick={() => handleDeleteModule(m)}
                    aria-label="Delete module"
                  >
                    <Icons.trash size={16} />
                  </Button>
                  <button
                    onClick={() => setExpanded(isOpen ? null : m.id)}
                    className="focus-ring rounded-lg p-1.5 text-ink-400 transition-colors hover:bg-ink-100 hover:text-ink-700"
                    aria-label={isOpen ? 'Hide activities' : 'Show activities'}
                  >
                    <Icons.chevronDown size={17} className={`transition-transform ${isOpen ? 'rotate-180' : ''}`} />
                  </button>
                </div>
                <AnimatePresence initial={false}>
                  {isOpen && (
                    <motion.div
                      initial={{ height: 0, opacity: 0 }}
                      animate={{ height: 'auto', opacity: 1 }}
                      exit={{ height: 0, opacity: 0 }}
                      className="overflow-hidden border-t border-ink-100"
                    >
                      <div className="bg-ink-50/60 p-4">
                        <ActivitiesPanel moduleId={m.id} />
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </Card>
            );
          })}
        </div>
      )}

      <ModuleFormModal
        open={moduleModal.isOpen}
        onClose={moduleModal.close}
        trainingId={id}
        module={moduleModal.payload}
        nextOrder={nextOrder}
        onSaved={() => {
          modules.refetch();
          training.refetch();
        }}
      />
    </div>
  );
}

function Meta({ icon, label }: { icon: React.ReactNode; label: string }) {
  return (
    <span className="inline-flex items-center gap-2">
      <span className="text-ink-400">{icon}</span>
      {label}
    </span>
  );
}

function toUpdate(m: ModuleDto) {
  return {
    title: m.title,
    description: m.description ?? null,
    order: m.order,
    duration: m.duration,
    videoUrl: m.videoUrl ?? null,
    attachment: m.attachment ?? null,
    aiAvatarEnabled: m.aiAvatarEnabled,
  };
}
