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

  // Default to the first incomplete module (or the first module).
  useEffect(() => {
    if (activeModuleId || !progress.data) return;
    const ordered = [...progress.data.modules].sort((a, b) => a.order - b.order);
    const firstIncomplete = ordered.find((m) => !m.completed) ?? ordered[0];
    if (firstIncomplete) setActiveModuleId(firstIncomplete.moduleId);
  }, [progress.data, activeModuleId]);

  if (progress.loading) return <FullPageLoader label="Loading course…" />;
  if (progress.error) return <ErrorState error={progress.error} onRetry={progress.refetch} />;
  const p = progress.data;
  if (!p) return null;

  const orderedModules = [...p.modules].sort((a, b) => a.order - b.order);
  const moduleMeta = (id: string): ModuleDto | undefined => modules.data?.find((m) => m.id === id);
  const isCompleted = p.status === EnrollmentStatus.Completed;

  return (
    <div>
      <Breadcrumbs items={[{ label: 'My Learning', to: '/my-learning' }, { label: p.trainingTitle ?? 'Course' }]} />

      <div className="mb-6 rounded-xl border border-slate-200 bg-white p-5 shadow-card">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h1 className="text-xl font-bold text-slate-900">{p.trainingTitle}</h1>
            <p className="text-sm text-slate-500">
              {orderedModules.filter((m) => m.completed).length} of {orderedModules.length} modules completed
            </p>
          </div>
          {isCompleted && (
            <Badge className="bg-emerald-100 text-emerald-700">
              <Icons.award size={14} /> Completed
            </Badge>
          )}
        </div>
        <div className="mt-3">
          <ProgressBar value={p.progressPercent} showLabel />
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-[280px_1fr]">
        {/* Module list */}
        <aside className="space-y-2">
          <h2 className="px-1 text-xs font-semibold uppercase tracking-wide text-slate-400">Modules</h2>
          {orderedModules.map((m) => (
            <button
              key={m.moduleId}
              onClick={() => setActiveModuleId(m.moduleId)}
              className={cn(
                'flex w-full items-center gap-3 rounded-lg border px-3 py-2.5 text-left transition-colors',
                activeModuleId === m.moduleId
                  ? 'border-brand-300 bg-brand-50'
                  : 'border-slate-200 bg-white hover:bg-slate-50',
              )}
            >
              <span
                className={cn(
                  'flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-xs font-semibold',
                  m.completed ? 'bg-emerald-500 text-white' : 'bg-slate-100 text-slate-500',
                )}
              >
                {m.completed ? <Icons.check size={15} /> : m.order}
              </span>
              <span className="min-w-0 flex-1">
                <span className="block truncate text-sm font-medium text-slate-700">{m.title}</span>
                <span className="text-xs text-slate-400">{m.completed ? 'Completed' : 'Not started'}</span>
              </span>
            </button>
          ))}

          {isCompleted && <CertificateCard trainingId={trainingId} />}
        </aside>

        {/* Active module content */}
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
              <CardBody className="py-12 text-center text-slate-400">Select a module to begin.</CardBody>
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
      notify.success('Module marked as completed.');
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
              <div>
                <h2 className="text-lg font-semibold text-slate-900">{module.title}</h2>
                {module.description && <p className="mt-1 text-sm text-slate-500">{module.description}</p>}
              </div>
              {module.aiAvatarEnabled && (
                <Badge className="bg-violet-100 text-violet-700">
                  <Icons.ai size={13} /> AI enabled
                </Badge>
              )}
            </div>
            {module.videoUrl && (
              <a
                href={module.videoUrl}
                target="_blank"
                rel="noreferrer"
                className="mt-3 inline-flex items-center gap-2 text-sm font-medium text-brand-600 hover:underline"
              >
                <Icons.video size={16} /> Watch module video
              </a>
            )}
            {module.attachment && (
              <a
                href={module.attachment}
                target="_blank"
                rel="noreferrer"
                className="mt-1 flex items-center gap-2 text-sm font-medium text-brand-600 hover:underline"
              >
                <Icons.document size={16} /> Download attachment
              </a>
            )}
          </CardBody>
        </Card>
      )}

      {module?.aiAvatarEnabled && <AITrainerPanel trainingId={trainingId} moduleId={moduleId} compact />}

      {loading ? (
        <div className="flex justify-center py-10 text-slate-400">
          <Spinner />
        </div>
      ) : ordered.length === 0 ? (
        <Card>
          <CardBody className="py-8 text-center text-sm text-slate-400">No activities in this module yet.</CardBody>
        </Card>
      ) : (
        ordered.map((a) =>
          a.type === ActivityType.Quiz || a.type === ActivityType.Exam ? (
            <QuizRunner key={a.id} trainingId={trainingId} activity={a} onCompleted={() => refetch()} />
          ) : (
            <Card key={a.id}>
              <CardBody>
                <div className="mb-2 flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-50 text-brand-600">
                    {a.type === ActivityType.Lesson ? <Icons.book size={17} /> : <Icons.edit size={17} />}
                  </span>
                  <h3 className="font-semibold text-slate-800">{a.title}</h3>
                </div>
                {a.type === ActivityType.Lesson ? (
                  <>
                    {a.content && <p className="whitespace-pre-line text-sm text-slate-600">{a.content}</p>}
                    {a.videoUrl && (
                      <a
                        href={a.videoUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="mt-2 inline-flex items-center gap-2 text-sm font-medium text-brand-600 hover:underline"
                      >
                        <Icons.play size={15} /> Watch lesson
                      </a>
                    )}
                  </>
                ) : (
                  <div className="space-y-2 text-sm text-slate-600">
                    {a.instructions && (
                      <div>
                        <p className="font-medium text-slate-700">Instructions</p>
                        <p className="whitespace-pre-line">{a.instructions}</p>
                      </div>
                    )}
                    {a.expectedOutcome && (
                      <div>
                        <p className="font-medium text-slate-700">Expected outcome</p>
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

      <div className="flex items-center justify-end gap-3 pt-2">
        {completed ? (
          <span className="inline-flex items-center gap-2 text-sm font-medium text-emerald-600">
            <Icons.check size={18} /> Module completed
          </span>
        ) : (
          <Button loading={completing} onClick={complete} leftIcon={<Icons.check size={17} />}>
            Mark module as complete
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
    <Card className="mt-4 border-emerald-200 bg-emerald-50/50">
      <CardBody className="text-center">
        <Icons.award size={28} className="mx-auto text-emerald-500" />
        <p className="mt-2 text-sm font-semibold text-slate-800">Certificate</p>
        <p className="mt-0.5 text-xs text-slate-500">{data.message ?? (data.available ? 'Available' : 'In progress')}</p>
        <Button className="mt-3" size="sm" variant="outline" disabled={!data.available}>
          {data.available ? 'View certificate' : 'Not yet available'}
        </Button>
      </CardBody>
    </Card>
  );
}
