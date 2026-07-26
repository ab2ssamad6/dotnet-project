import { useState } from 'react';
import { Card, CardBody, Icons, Select } from '@/components/ui';
import { PageHeader } from '@/components/common/PageHeader';
import { AITrainerPanel } from '@/features/ai/AITrainerPanel';
import { useAsync } from '@/hooks/useAsync';
import { moduleService, trainingService } from '@/services';

export function AITrainerPage() {
  const [trainingId, setTrainingId] = useState('');
  const [moduleId, setModuleId] = useState('');

  const trainings = useAsync(() => trainingService.list({ page: 1, pageSize: 100 }), []);
  const modules = useAsync(
    () => (trainingId ? moduleService.listByTraining(trainingId) : Promise.resolve([])),
    [trainingId],
  );

  return (
    <div>
      <PageHeader
        title="AI Trainer"
        description="Chat with an AI-powered avatar trainer. Optionally scope the session to a specific module."
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
                onChange={(e) => {
                  setTrainingId(e.target.value);
                  setModuleId('');
                }}
                options={(trainings.data?.items ?? []).map((t) => ({ value: t.id, label: t.title }))}
              />
              <Select
                label="Module (optional)"
                placeholder={trainingId ? 'Any module' : 'Select a training first'}
                value={moduleId}
                onChange={(e) => setModuleId(e.target.value)}
                disabled={!trainingId || modules.loading}
                options={[...(modules.data ?? [])]
                  .sort((a, b) => a.order - b.order)
                  .map((m) => ({ value: m.id, label: `${m.order}. ${m.title}` }))}
              />
              <div className="flex items-start gap-2 rounded-lg bg-slate-50 p-3 text-xs text-slate-500">
                <Icons.info size={16} className="mt-0.5 shrink-0" />
                <p>Scoping to a module primes the avatar with that module's context when available.</p>
              </div>
            </CardBody>
          </Card>
        </div>

        <AITrainerPanel key={moduleId || trainingId || 'global'} moduleId={moduleId || null} />
      </div>
    </div>
  );
}
