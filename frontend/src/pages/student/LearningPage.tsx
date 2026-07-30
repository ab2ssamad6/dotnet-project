import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Badge, Button, Card, CardBody, Icons, ProgressBar, Spinner } from '@/components/ui';
import { Breadcrumbs } from '@/components/common/Breadcrumbs';
import { ErrorState } from '@/components/common/ErrorState';
import { FullPageLoader } from '@/components/common/FullPageLoader';
import { AITrainerPanel } from '@/features/ai/AITrainerPanel';
import { useAsync } from '@/hooks/useAsync';
import { activityService, enrollmentService, moduleService } from '@/services';
import { notify } from '@/utils/toast';
import { cn } from '@/utils/cn';
import { ActivityType, EnrollmentStatus, type ModuleDto } from '@/types';
import { QuizRunner } from './QuizRunner';

export function LearningPage() {
  const { trainingId = '' } = useParams();
  const progress = useAsync(() => enrollmentService.progress(trainingId), [trainingId]);
  const modules = useAsync(() => moduleService.listByTraining(trainingId), [trainingId]);
  const [activeModuleId, setActiveModuleId] = useState<string | null>(null);

  useEffect(() => {
    if (activeModuleId || !progress.data) return;
    const ordered = [...progress.data.modules].sort((a, b) => a.order - b.order);
    const firstIncomplete = ordered.find((m) => !m.completed) ?? ordered[0];
    if (firstIncomplete) setActiveModuleId(firstIncomplete.moduleId);
  }, [progress.data, activeModuleId]);

  if (progress.loading) return <FullPageLoader label="Loading your course…" />;
  if (progress.error) return <ErrorState error={progress.error} onRetry={progress.refetch} />;
  const p = progress.data;
  if (!p) return null;

  const orderedModules = [...p.modules].sort((a, b) => a.order - b.order);
  const moduleMeta = (id: string): ModuleDto | undefined => modules.data?.find((m) => m.id === id);
  const isCompleted = p.status === EnrollmentStatus.Completed;
  const doneCount = orderedModules.filter((m) => m.completed).length;

  return (
    <div>
      <Breadcrumbs items={[{ label: 'My Learning', to: '/my-learning' }, { label: p.trainingTitle ?? 'Course' }]} />

      <div className="surface mb-7 overflow-hidden">
        <div className="flex flex-wrap items-center justify-between gap-4 p-6">
          <div className="min-w-0">
            <p className="eyebrow">Course in progress</p>
            <h1 className="mt-2 font-display text-[26px] font-semibold leading-tight tracking-[-0.02em] text-ink-900">
              {p.trainingTitle}
            </h1>
            <p className="mt-2 text-sm text-ink-500">
              <span className="tnum font-bold text-ink-800">{doneCount}</span> of{' '}
              <span className="tnum font-bold text-ink-800">{orderedModules.length}</span> modules completed
            </p>
          </div>
          {isCompleted && (
            <Badge tone="success">
              <Icons.award size={13} /> Course completed
            </Badge>
          )}
        </div>
        <div className="border-t border-ink-100 px-6 py-4">
          <ProgressBar value={p.progressPercent} showLabel />
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-[300px_1fr]">
        <aside className="lg:sticky lg:top-[92px] lg:self-start">
          <p className="eyebrow mb-3 px-1">Modules</p>
          <div className="space-y-2">
            {orderedModules.map((m) => {
              const isActive = activeModuleId === m.moduleId;
              return (
                <button
                  key={m.moduleId}
                  onClick={() => setActiveModuleId(m.moduleId)}
                  className={cn(
                    'focus-ring flex w-full items-center gap-3 rounded-xl border px-3.5 py-3 text-left transition-all',
                    isActive
                      ? 'border-brand-300 bg-brand-50/70 shadow-card'
                      : 'border-ink-200/80 bg-white hover:border-ink-300/80 hover:bg-ink-50/60',
                  )}
                >
                  <span
                    className={cn(
                      'tnum flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-[11.5px] font-bold',
                      m.completed
                        ? 'bg-green-600 text-white'
                        : isActive
                          ? 'bg-brand-700 text-white'
                          : 'bg-ink-100 text-ink-500',
                    )}
                  >
                    {m.completed ? <Icons.check size={14} /> : m.order}
                  </span>
                  <span className="min-w-0 flex-1">
                    <span className="block truncate text-[13.5px] font-bold text-ink-800">{m.title}</span>
                    <span
                      className={cn(
                        'text-[11.5px] font-semibold',
                        m.completed ? 'text-green-600' : 'text-ink-400',
                      )}
                    >
                      {m.completed ? 'Completed' : isActive ? 'Working on this' : 'Not started'}
                    </span>
                  </span>
                </button>
              );
            })}
          </div>

          {isCompleted && <CertificateCard trainingId={trainingId} />}
        </aside>

        <div>
          {activeModuleId ? (
            <ModuleContent
              key={activeModuleId}
              trainingId={trainingId}
              moduleId={activeModuleId}
              module={moduleMeta(activeModuleId)}
              completed={orderedModules.find((m) => m.moduleId === activeModuleId)?.completed ?? false}
              onProgressChanged={() => {
                progress.refetch();
              }}
            />
          ) : (
            <Card>
              <CardBody className="py-14 text-center text-sm text-ink-400">
                Pick a module on the left to get started.
              </CardBody>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

function ModuleContent({
  trainingId,
  moduleId,
  module,
  completed,
  onProgressChanged,
}: {
  trainingId: string;
  moduleId: string;
  module?: ModuleDto;
  completed: boolean;
  onProgressChanged: () => void;
}) {
  const { data: activities, loading, refetch } = useAsync(() => activityService.listByModule(moduleId), [moduleId]);
  const [completing, setCompleting] = useState(false);

  const ordered = [...(activities ?? [])].sort((a, b) => a.order - b.order);

  const complete = async () => {
    setCompleting(true);
    try {
      await enrollmentService.completeModule(trainingId, moduleId);
      notify.success('Module completed — nice work.');
      onProgressChanged();
    } catch (err) {
      notify.apiError(err);
    } finally {
      setCompleting(false);
    }
  };

  return (
    <div className="space-y-4">
      {module && (
        <Card>
          <CardBody>
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0">
                <h2 className="font-display text-[20px] font-semibold tracking-[-0.02em] text-ink-900">
                  {module.title}
                </h2>
                {module.description && (
                  <p className="mt-2 text-sm leading-relaxed text-ink-500">{module.description}</p>
                )}
              </div>
              {module.aiAvatarEnabled && (
                <Badge tone="ai">
                  <Icons.sparkle size={12} /> AI tutor
                </Badge>
              )}
            </div>
            {(module.videoUrl || module.attachment) && (
              <div className="mt-4 flex flex-wrap gap-2 border-t border-ink-100 pt-4">
                {module.videoUrl && (
                  <a
                    href={module.videoUrl}
                    target="_blank"
                    rel="noreferrer"
                    className="focus-ring inline-flex items-center gap-2 rounded-lg border border-ink-200 bg-white px-3 py-2 text-[13px] font-semibold text-ink-700 shadow-card transition-colors hover:border-ink-300 hover:bg-ink-50"
                  >
                    <Icons.video size={15} className="text-brand-600" /> Watch module video
                    <Icons.external size={13} className="text-ink-400" />
                  </a>
                )}
                {module.attachment && (
                  <a
                    href={module.attachment}
                    target="_blank"
                    rel="noreferrer"
                    className="focus-ring inline-flex items-center gap-2 rounded-lg border border-ink-200 bg-white px-3 py-2 text-[13px] font-semibold text-ink-700 shadow-card transition-colors hover:border-ink-300 hover:bg-ink-50"
                  >
                    <Icons.document size={15} className="text-brand-600" /> Download handout
                    <Icons.external size={13} className="text-ink-400" />
                  </a>
                )}
              </div>
            )}
          </CardBody>
        </Card>
      )}

      {module?.aiAvatarEnabled && <AITrainerPanel trainingId={trainingId} moduleId={moduleId} compact />}

      {loading ? (
        <div className="flex justify-center py-12 text-ink-400">
          <Spinner />
        </div>
      ) : ordered.length === 0 ? (
        <Card>
          <CardBody className="py-10 text-center text-sm text-ink-400">
            This module has no activities yet — you can still mark it complete.
          </CardBody>
        </Card>
      ) : (
        ordered.map((a) =>
          a.type === ActivityType.Quiz || a.type === ActivityType.Exam ? (
            <QuizRunner key={a.id} trainingId={trainingId} activity={a} onCompleted={() => refetch()} />
          ) : (
            <Card key={a.id}>
              <CardBody>
                <div className="mb-3 flex items-center gap-2.5">
                  <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-50 text-brand-700 ring-1 ring-inset ring-brand-200/60">
                    {a.type === ActivityType.Lesson ? <Icons.book size={16} /> : <Icons.edit size={16} />}
                  </span>
                  <h3 className="text-[15px] font-bold tracking-[-0.01em] text-ink-900">{a.title}</h3>
                </div>
                {a.type === ActivityType.Lesson ? (
                  <>
                    {a.content && (
                      <p className="whitespace-pre-line text-[14px] leading-relaxed text-ink-600">{a.content}</p>
                    )}
                    {a.videoUrl && (
                      <a
                        href={a.videoUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="mt-3 inline-flex items-center gap-2 text-[13px] font-semibold text-brand-700 transition-colors hover:text-brand-800"
                      >
                        <Icons.play size={14} /> Watch this lesson
                      </a>
                    )}
                  </>
                ) : (
                  <div className="space-y-3 text-[14px] leading-relaxed text-ink-600">
                    {a.instructions && (
                      <div>
                        <p className="eyebrow mb-1">Instructions</p>
                        <p className="whitespace-pre-line">{a.instructions}</p>
                      </div>
                    )}
                    {a.expectedOutcome && (
                      <div className="rounded-xl border border-ink-200/80 bg-ink-50/70 p-3.5">
                        <p className="eyebrow mb-1">Expected outcome</p>
                        <p className="whitespace-pre-line">{a.expectedOutcome}</p>
                      </div>
                    )}
                  </div>
                )}
              </CardBody>
            </Card>
          ),
        )
      )}

      <div className="flex items-center justify-end gap-3 pt-1">
        {completed ? (
          <span className="inline-flex items-center gap-2 rounded-lg bg-green-50 px-3.5 py-2 text-[13px] font-bold text-green-700 ring-1 ring-inset ring-green-200/70">
            <Icons.check size={16} /> Module completed
          </span>
        ) : (
          <Button loading={completing} onClick={complete} leftIcon={<Icons.check size={16} />}>
            Mark module complete
          </Button>
        )}
      </div>
    </div>
  );
}

function CertificateCard({ trainingId }: { trainingId: string }) {
  const { data, loading } = useAsync(() => enrollmentService.certificate(trainingId), [trainingId]);
  if (loading || !data) return null;
  return (
    <Card className="mt-5 border-gold-200 bg-gold-50/60">
      <CardBody className="text-center">
        <span className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-2xl bg-gold-gradient text-ink-900 shadow-card">
          <Icons.award size={22} />
        </span>
        <p className="text-sm font-bold text-ink-900">Certificate</p>
        <p className="mt-1 text-[12.5px] leading-relaxed text-ink-500">
          {data.message ?? (data.available ? 'Ready to download.' : 'Finish every module to unlock it.')}
        </p>
        <Button className="mt-4" size="sm" variant="outline" disabled={!data.available}>
          {data.available ? 'View certificate' : 'Not yet available'}
        </Button>
      </CardBody>
    </Card>
  );
}
