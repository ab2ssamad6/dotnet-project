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
        title="AI Trainer"
        description="Chat with an AI-powered avatar trainer. Pick a training and it will tutor you on that subject."
      />

      <div className="grid gap-6 lg:grid-cols-[320px_1fr]">
        <div className="space-y-4">
          <Card>
            <CardBody className="space-y-4">
              <h3 className="text-sm font-semibold text-slate-700">Session context</h3>
              <Select
                label="Training (optional)"
                placeholder="Any training"
                value={trainingId}
                onChange={(e) => setScope({ trainingId: e.target.value, moduleId: '' })}
                options={(trainings.data?.items ?? []).map((t) => ({ value: t.id, label: t.title }))}
              />
              <Select
                label="Module (optional)"
                placeholder={trainingId ? 'Whole training' : 'Select a training first'}
                value={moduleId}
                onChange={(e) => setScope({ trainingId, moduleId: e.target.value })}
                disabled={!trainingId || modules.loading}
                options={[...(modules.data ?? [])]
                  .sort((a, b) => a.order - b.order)
                  .map((m) => ({ value: m.id, label: `${m.order}. ${m.title}` }))}
              />
              <div className="flex items-start gap-2 rounded-lg bg-slate-50 p-3 text-xs text-slate-500">
                <Icons.info size={16} className="mt-0.5 shrink-0" />
                <p>
                  {trainingId
                    ? moduleId
                      ? 'The avatar introduces itself as tutor of this training and focuses on the selected module.'
                      : 'The avatar introduces itself as tutor of this training and knows its full curriculum.'
                    : 'Pick a training to have the avatar introduce itself as its tutor and teach its curriculum.'}
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
