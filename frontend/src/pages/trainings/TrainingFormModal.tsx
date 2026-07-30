import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Button, Checkbox, Input, Modal, Select, Textarea } from '@/components/ui';
import { zodResolver } from '@/utils/zodResolver';
import { DIFFICULTY_OPTIONS, TRAINING_STATUS_OPTIONS } from '@/constants/enums';
import { useLookups } from '@/hooks/useLookups';
import { trainingService } from '@/services';
import { notify } from '@/utils/toast';
import { DifficultyLevel, TrainingStatus, type TrainingDto, type TrainingRequest } from '@/types';

const schema = z.object({
  title: z.string().min(1, 'Title is required.').max(200),
  description: z.string().min(1, 'Description is required.'),
  categoryId: z.string().min(1, 'Select a category.'),
  trainerId: z.string().min(1, 'Select a trainer.'),
  difficulty: z.coerce.number().int().min(0).max(3),
  status: z.coerce.number().int().min(0).max(2),
  duration: z.coerce.number().int().min(0, 'Duration must be 0 or more.'),
  thumbnail: z.string().url('Enter a valid URL.').optional().or(z.literal('')),
  published: z.boolean(),
});
type FormValues = z.infer<typeof schema>;

interface Props {
  open: boolean;
  onClose: () => void;
  training?: TrainingDto | null;
  onSaved: () => void;
}

export function TrainingFormModal({ open, onClose, training, onSaved }: Props) {
  const { categoryOptions, trainerOptions, loading: lookupsLoading } = useLookups();

  const {
    register,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      title: '',
      description: '',
      categoryId: '',
      trainerId: '',
      difficulty: DifficultyLevel.Beginner,
      status: TrainingStatus.Draft,
      duration: 60,
      thumbnail: '',
      published: false,
    },
  });

  useEffect(() => {
    if (!open) return;
    if (training) {
      reset({
        title: training.title,
        description: training.description,
        categoryId: training.categoryId,
        trainerId: training.trainerId,
        difficulty: training.difficulty,
        status: training.status,
        duration: training.duration,
        thumbnail: training.thumbnail ?? '',
        published: training.published,
      });
    } else {
      reset({
        title: '',
        description: '',
        categoryId: '',
        trainerId: '',
        difficulty: DifficultyLevel.Beginner,
        status: TrainingStatus.Draft,
        duration: 60,
        thumbnail: '',
        published: false,
      });
    }
  }, [open, training, reset]);

  const status = watch('status');
  useEffect(() => {
    if (Number(status) === TrainingStatus.Published) setValue('published', true);
    if (Number(status) === TrainingStatus.Draft) setValue('published', false);
  }, [status, setValue]);

  const onSubmit = async (values: FormValues) => {
    const payload: TrainingRequest = {
      title: values.title,
      description: values.description,
      categoryId: values.categoryId,
      trainerId: values.trainerId,
      difficulty: values.difficulty,
      status: values.status,
      duration: values.duration,
      thumbnail: values.thumbnail || null,
      published: values.published,
    };
    try {
      if (training) {
        await trainingService.update(training.id, payload);
        notify.success('Training updated.');
      } else {
        await trainingService.create(payload);
        notify.success('Training created.');
      }
      onSaved();
      onClose();
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <Modal
      open={open}
      onClose={onClose}
      size="xl"
      title={training ? 'Edit training' : 'New training'}
      closeOnBackdrop={!isSubmitting}
      footer={
        <>
          <Button variant="outline" onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button onClick={handleSubmit(onSubmit)} loading={isSubmitting}>
            {training ? 'Save changes' : 'Create training'}
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input label="Title" placeholder="e.g. Building APIs with ASP.NET Core" error={errors.title?.message} required {...register('title')} />
        <Textarea
          label="Description"
          rows={4}
          placeholder="What learners will achieve…"
          error={errors.description?.message}
          required
          {...register('description')}
        />
        <div className="grid gap-4 sm:grid-cols-2">
          <Select
            label="Category"
            placeholder={lookupsLoading ? 'Loading…' : 'Select a category'}
            options={categoryOptions}
            error={errors.categoryId?.message}
            required
            {...register('categoryId')}
          />
          <Select
            label="Trainer"
            placeholder={lookupsLoading ? 'Loading…' : 'Select a trainer'}
            options={trainerOptions}
            error={errors.trainerId?.message}
            required
            {...register('trainerId')}
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-3">
          <Select
            label="Difficulty"
            options={DIFFICULTY_OPTIONS.map((o) => ({ value: o.value, label: o.label }))}
            error={errors.difficulty?.message}
            {...register('difficulty')}
          />
          <Select
            label="Status"
            options={TRAINING_STATUS_OPTIONS.map((o) => ({ value: o.value, label: o.label }))}
            error={errors.status?.message}
            {...register('status')}
          />
          <Input label="Duration (min)" type="number" min={0} error={errors.duration?.message} {...register('duration')} />
        </div>
        <Input
          label="Thumbnail URL"
          placeholder="https://…/cover.jpg"
          error={errors.thumbnail?.message}
          {...register('thumbnail')}
        />
        <div className="rounded-lg border border-slate-200 bg-slate-50 p-3">
          <Checkbox
            label="Published"
            description="Published trainings appear in the student catalog and can be enrolled in."
            {...register('published')}
          />
        </div>
      </form>
    </Modal>
  );
}
