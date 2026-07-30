import { useCallback } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Card, CardBody, Icons, Select } from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { AITrainerPanel } from '@/features/ai/AITrainerPanel';
import { useAsync } from '@/hooks/useAsync';
import { useAuth } from '@/hooks/useAuth';
import { moduleService, trainingService } from '@/services';

export function AITrainerPage() {
  const { isStudent } = useAuth();
  const [params, setParams] = useSearchParams();
  const trainingId = params.get('trainingId') ?? '';
  const moduleId = params.get('moduleId') ?? '';

  const setScope = useCallback(
    (next: { trainingId: string; moduleId: string }) => {
      const updated = new URLSearchParams(params);
      if (next.trainingId) updated.set('trainingId', next.trainingId);
      else updated.delete('trainingId');
      if (next.moduleId) updated.set('moduleId', next.moduleId);
      else updated.delete('moduleId');
      setParams(updated, { replace: true });
    },
    [params, setParams],
  );

  const trainings = useAsync(
    () =>
      isStudent
        ? trainingService.catalog({ page: 1, pageSize: 100 })
        : trainingService.list({ page: 1, pageSize: 100 }),
    [isStudent],
  );
  const modules = useAsync(
    () => (trainingId ? moduleService.listByTraining(trainingId) : Promise.resolve([])),
    [trainingId],
  );

  const selectedTraining = (trainings.data?.items ?? []).find((t) => t.id === trainingId);

  return (
    <div>
      <PageHeader
        eyebrow="Workspace"
        title="AI Trainer"
        description="A conversational avatar that tutors on your course material. Choose what it should focus on, then start the session."
      />

      <div className="grid gap-5 lg:grid-cols-[340px_1fr]">
        <div className="space-y-4 lg:sticky lg:top-[92px] lg:self-start">
          <Card>
            <CardBody className="space-y-4">
              <div className="flex items-center gap-2.5">
                <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-violet-50 text-violet-600 ring-1 ring-inset ring-violet-200/60">
                  <Icons.target size={16} />
                </span>
                <h3 className="text-sm font-bold tracking-[-0.01em] text-ink-900">Session focus</h3>
              </div>
              <Select
                label="Training"
                placeholder="Any training"
                value={trainingId}
                onChange={(e) => setScope({ trainingId: e.target.value, moduleId: '' })}
                options={(trainings.data?.items ?? []).map((t) => ({ value: t.id, label: t.title }))}
              />
              <Select
                label="Module"
                placeholder={trainingId ? 'The whole training' : 'Pick a training first'}
                value={moduleId}
                onChange={(e) => setScope({ trainingId, moduleId: e.target.value })}
                disabled={!trainingId || modules.loading}
                options={[...(modules.data ?? [])]
                  .sort((a, b) => a.order - b.order)
                  .map((m) => ({ value: m.id, label: `${m.order}. ${m.title}` }))}
              />
              <div className="flex items-start gap-2.5 rounded-xl border border-ink-200/80 bg-ink-50/70 p-3.5 text-[12.5px] leading-relaxed text-ink-500">
                <Icons.info size={15} className="mt-0.5 shrink-0 text-ink-400" />
                <p>
                  {trainingId
                    ? moduleId
                      ? 'The avatar introduces itself as the tutor for this training and stays on the selected module.'
                      : 'The avatar introduces itself as the tutor for this training and knows the full curriculum.'
                    : 'Pick a training and the avatar will introduce itself as its tutor and teach from the curriculum.'}
                </p>
              </div>
            </CardBody>
          </Card>
        </div>

        <AITrainerPanel
          key={`${trainingId}:${moduleId}`}
          trainingId={trainingId || null}
          moduleId={moduleId || null}
          subjectLabel={selectedTraining?.title ?? null}
        />
      </div>
    </div>
  );
}
