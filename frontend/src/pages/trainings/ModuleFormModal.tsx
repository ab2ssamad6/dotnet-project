import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Button, Checkbox, Input, Modal, Textarea } from '@/components/ui';
import { zodResolver } from '@/utils/zodResolver';
import { moduleService } from '@/services';
import { notify } from '@/utils/toast';
import type { ModuleDto } from '@/types';

const schema = z.object({
  title: z.string().min(1, 'Title is required.').max(200),
  description: z.string().max(1000).optional(),
  order: z.coerce.number().int().min(0),
  duration: z.coerce.number().int().min(0),
  videoUrl: z.string().url('Enter a valid URL.').optional().or(z.literal('')),
  attachment: z.string().url('Enter a valid URL.').optional().or(z.literal('')),
  aiAvatarEnabled: z.boolean(),
});
type FormValues = z.infer<typeof schema>;

interface Props {
  open: boolean;
  onClose: () => void;
  trainingId: string;
  module?: ModuleDto | null;
  nextOrder: number;
  onSaved: () => void;
}

export function ModuleFormModal({ open, onClose, trainingId, module, nextOrder, onSaved }: Props) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  useEffect(() => {
    if (!open) return;
    reset({
      title: module?.title ?? '',
      description: module?.description ?? '',
      order: module?.order ?? nextOrder,
      duration: module?.duration ?? 30,
      videoUrl: module?.videoUrl ?? '',
      attachment: module?.attachment ?? '',
      aiAvatarEnabled: module?.aiAvatarEnabled ?? false,
    });
  }, [open, module, nextOrder, reset]);

  const onSubmit = async (values: FormValues) => {
    try {
      if (module) {
        await moduleService.update(module.id, {
          title: values.title,
          description: values.description || null,
          order: values.order,
          duration: values.duration,
          videoUrl: values.videoUrl || null,
          attachment: values.attachment || null,
          aiAvatarEnabled: values.aiAvatarEnabled,
        });
        notify.success('Module updated.');
      } else {
        await moduleService.create({
          trainingId,
          title: values.title,
          description: values.description || null,
          order: values.order,
          duration: values.duration,
          videoUrl: values.videoUrl || null,
          attachment: values.attachment || null,
          aiAvatarEnabled: values.aiAvatarEnabled,
        });
        notify.success('Module added.');
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
      size="lg"
      title={module ? 'Edit module' : 'New module'}
      description="Modules group the lessons, exercises and assessments learners work through in order."
      closeOnBackdrop={!isSubmitting}
      footer={
        <>
          <Button variant="outline" onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button onClick={handleSubmit(onSubmit)} loading={isSubmitting}>
            {module ? 'Save changes' : 'Add module'}
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <Input label="Title" placeholder="e.g. Getting Started" error={errors.title?.message} required {...register('title')} />
        <Textarea label="Description" rows={3} error={errors.description?.message} {...register('description')} />
        <div className="grid gap-4 sm:grid-cols-2">
          <Input label="Order" type="number" min={0} hint="Position within the training." error={errors.order?.message} {...register('order')} />
          <Input label="Duration (min)" type="number" min={0} error={errors.duration?.message} {...register('duration')} />
        </div>
        <Input label="Video URL" placeholder="https://…" error={errors.videoUrl?.message} {...register('videoUrl')} />
        <Input label="Attachment URL" placeholder="https://…/handout.pdf" error={errors.attachment?.message} {...register('attachment')} />
        <div className="rounded-xl border border-ink-200/80 bg-ink-50/70 p-4">
          <Checkbox
            label="Enable the AI avatar"
            description="Lets the AI Trainer present and answer questions on this module."
            {...register('aiAvatarEnabled')}
          />
        </div>
      </form>
    </Modal>
  );
}
