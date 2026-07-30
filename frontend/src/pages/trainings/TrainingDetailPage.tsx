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
      title: 'Delete module',
      message: (
        <>
          Delete <span className="font-medium">"{module.title}"</span> and all its activities?
        </>
      ),
      confirmLabel: 'Delete',
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

      <Card className="mb-6 overflow-hidden">
        <div className="relative h-36 bg-gradient-to-br from-brand-600 to-brand-800">
          {t.thumbnail && <img src={t.thumbnail} alt="" className="h-full w-full object-cover opacity-60" />}
          <div className="absolute inset-0 flex items-end justify-between p-5">
            <div className="flex items-center gap-2">
              <TrainingStatusBadge value={t.status} />
              <DifficultyBadge value={t.difficulty} />
            </div>
            <div className="flex gap-2">
              <Button
                variant="secondary"
                size="sm"
                leftIcon={<Icons.ai size={15} />}
                onClick={() => navigate(`/ai-trainer?trainingId=${id}`)}
              >
                Train with AI
              </Button>
              <Button variant="secondary" size="sm" leftIcon={<Icons.edit size={15} />} onClick={() => navigate('/trainings')}>
                Back
              </Button>
            </div>
          </div>
        </div>
        <CardBody>
          <h1 className="text-xl font-bold text-slate-900">{t.title}</h1>
          <p className="mt-1 text-sm text-slate-500">{t.description}</p>
          <div className="mt-4 flex flex-wrap gap-4 text-sm text-slate-500">
            <span className="inline-flex items-center gap-1.5">
              <Icons.category size={16} /> {t.categoryName ?? 'Uncategorized'}
            </span>
            <span className="inline-flex items-center gap-1.5">
              <Icons.trainer size={16} /> {t.trainerName ?? '—'}
            </span>
            <span className="inline-flex items-center gap-1.5">
              <Icons.clock size={16} /> {formatDuration(t.duration)}
            </span>
            <span className="inline-flex items-center gap-1.5">
              <Icons.layers size={16} /> {ordered.length} modules
            </span>
          </div>
        </CardBody>
      </Card>

      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold text-slate-900">Modules</h2>
        <Button leftIcon={<Icons.plus size={18} />} onClick={() => moduleModal.open()}>
          Add module
        </Button>
      </div>

      {modules.loading ? (
        <div className="flex justify-center py-10 text-slate-400">
          <Spinner />
        </div>
      ) : ordered.length === 0 ? (
        <Card>
          <CardBody className="py-10 text-center">
            <Icons.layers size={30} className="mx-auto mb-2 text-slate-300" />
            <p className="text-sm text-slate-500">No modules yet. Add the first module to build your curriculum.</p>
            <Button className="mt-4" onClick={() => moduleModal.open()}>
              Add module
            </Button>
          </CardBody>
        </Card>
      ) : (
        <div className="space-y-3">
          {ordered.map((m, i) => (
            <Card key={m.id}>
              <div className="flex items-center gap-3 px-4 py-3">
                <div className="flex flex-col">
                  <button
                    onClick={() => reorder(m, -1)}
                    disabled={i === 0}
                    className="text-slate-300 hover:text-slate-600 disabled:opacity-30"
                    aria-label="Move up"
                  >
                    <Icons.arrowUp size={14} />
                  </button>
                  <button
                    onClick={() => reorder(m, 1)}
                    disabled={i === ordered.length - 1}
                    className="text-slate-300 hover:text-slate-600 disabled:opacity-30"
                    aria-label="Move down"
                  >
                    <Icons.arrowDown size={14} />
                  </button>
                </div>
                <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-brand-50 text-sm font-semibold text-brand-600">
                  {m.order}
                </span>
                <button className="min-w-0 flex-1 text-left" onClick={() => setExpanded(expanded === m.id ? null : m.id)}>
                  <div className="flex items-center gap-2">
                    <p className="truncate font-medium text-slate-800">{m.title}</p>
                    {m.aiAvatarEnabled && (
                      <Badge className="bg-violet-100 text-violet-700">
                        <Icons.ai size={12} /> AI
                      </Badge>
                    )}
                  </div>
                  <p className="truncate text-xs text-slate-400">
                    {formatDuration(m.duration)}
                    {m.description ? ` · ${m.description}` : ''}
                  </p>
                </button>
                <Button variant="ghost" size="icon" onClick={() => moduleModal.open(m)} aria-label="Edit module">
                  <Icons.edit size={16} />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  className="text-rose-500 hover:bg-rose-50"
                  onClick={() => handleDeleteModule(m)}
                  aria-label="Delete module"
                >
                  <Icons.trash size={16} />
                </Button>
                <button
                  onClick={() => setExpanded(expanded === m.id ? null : m.id)}
                  className="text-slate-400"
                  aria-label="Toggle activities"
                >
                  <Icons.chevronDown
                    size={18}
                    className={`transition-transform ${expanded === m.id ? 'rotate-180' : ''}`}
                  />
                </button>
              </div>
              <AnimatePresence initial={false}>
                {expanded === m.id && (
                  <motion.div
                    initial={{ height: 0, opacity: 0 }}
                    animate={{ height: 'auto', opacity: 1 }}
                    exit={{ height: 0, opacity: 0 }}
                    className="overflow-hidden border-t border-slate-100"
                  >
                    <div className="bg-slate-50/50 p-4">
                      <ActivitiesPanel moduleId={m.id} />
                    </div>
                  </motion.div>
                )}
              </AnimatePresence>
            </Card>
          ))}
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
