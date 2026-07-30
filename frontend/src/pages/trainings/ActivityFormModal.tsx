import { useForm } from 'react-hook-form';
import { Button, Input, Modal, Textarea } from '@/components/ui';
import { zodResolver } from '@/utils/zodResolver';
import { activityService } from '@/services';
import { notify } from '@/utils/toast';
import { ActivityType, QuestionType } from '@/types';
import {
  assessmentSchema,
  exerciseSchema,
  lessonSchema,
  type AssessmentFormValues,
  type ExerciseFormValues,
  type LessonFormValues,
} from './activitySchemas';
import { QuestionsBuilder } from './QuestionsBuilder';

interface Props {
  open: boolean;
  onClose: () => void;
  moduleId: string;
  type: ActivityType;
  nextOrder: number;
  onSaved: () => void;
}

const TITLES: Record<ActivityType, string> = {
  [ActivityType.Lesson]: 'Add a lesson',
  [ActivityType.Exercise]: 'Add an exercise',
  [ActivityType.Quiz]: 'Add a quiz',
  [ActivityType.Exam]: 'Add an exam',
};

const SUBTITLES: Record<ActivityType, string> = {
  [ActivityType.Lesson]: 'Reading material and video for learners to work through.',
  [ActivityType.Exercise]: 'Hands-on practice with clear instructions and an expected outcome.',
  [ActivityType.Quiz]: 'A short scored check — set the pass mark and an optional time limit.',
  [ActivityType.Exam]: 'A formal assessment, scored automatically against your answer key.',
};

export function ActivityFormModal({ open, onClose, moduleId, type, nextOrder, onSaved }: Props) {
  const isAssessment = type === ActivityType.Quiz || type === ActivityType.Exam;
  return (
    <Modal
      open={open}
      onClose={onClose}
      size={isAssessment ? 'xl' : 'lg'}
      title={TITLES[type]}
      description={SUBTITLES[type]}
    >
      {type === ActivityType.Lesson && (
        <LessonForm moduleId={moduleId} nextOrder={nextOrder} onClose={onClose} onSaved={onSaved} />
      )}
      {type === ActivityType.Exercise && (
        <ExerciseForm moduleId={moduleId} nextOrder={nextOrder} onClose={onClose} onSaved={onSaved} />
      )}
      {isAssessment && (
        <AssessmentForm moduleId={moduleId} type={type} nextOrder={nextOrder} onClose={onClose} onSaved={onSaved} />
      )}
    </Modal>
  );
}

function Footer({ onClose, loading, label }: { onClose: () => void; loading: boolean; label: string }) {
  return (
    <div className="mt-7 flex items-center justify-end gap-3 border-t border-ink-100 pt-5">
      <Button type="button" variant="outline" onClick={onClose} disabled={loading}>
        Cancel
      </Button>
      <Button type="submit" loading={loading}>
        {label}
      </Button>
    </div>
  );
}

function LessonForm({
  moduleId,
  nextOrder,
  onClose,
  onSaved,
}: {
  moduleId: string;
  nextOrder: number;
  onClose: () => void;
  onSaved: () => void;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LessonFormValues>({
    resolver: zodResolver(lessonSchema),
    defaultValues: { title: '', order: nextOrder, content: '', videoUrl: '' },
  });

  const onSubmit = async (v: LessonFormValues) => {
    try {
      await activityService.createLesson(moduleId, {
        title: v.title,
        order: v.order,
        content: v.content || null,
        videoUrl: v.videoUrl || null,
      });
      notify.success('Lesson added.');
      onSaved();
      onClose();
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid gap-4 sm:grid-cols-[1fr_120px]">
        <Input label="Title" error={errors.title?.message} required {...register('title')} />
        <Input label="Order" type="number" min={0} error={errors.order?.message} {...register('order')} />
      </div>
      <Textarea label="Content" rows={5} placeholder="Lesson text / notes…" error={errors.content?.message} {...register('content')} />
      <Input label="Video URL" placeholder="https://…" error={errors.videoUrl?.message} {...register('videoUrl')} />
      <Footer onClose={onClose} loading={isSubmitting} label="Add lesson" />
    </form>
  );
}

function ExerciseForm({
  moduleId,
  nextOrder,
  onClose,
  onSaved,
}: {
  moduleId: string;
  nextOrder: number;
  onClose: () => void;
  onSaved: () => void;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ExerciseFormValues>({
    resolver: zodResolver(exerciseSchema),
    defaultValues: { title: '', order: nextOrder, instructions: '', expectedOutcome: '' },
  });

  const onSubmit = async (v: ExerciseFormValues) => {
    try {
      await activityService.createExercise(moduleId, {
        title: v.title,
        order: v.order,
        instructions: v.instructions || null,
        expectedOutcome: v.expectedOutcome || null,
      });
      notify.success('Exercise added.');
      onSaved();
      onClose();
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid gap-4 sm:grid-cols-[1fr_120px]">
        <Input label="Title" error={errors.title?.message} required {...register('title')} />
        <Input label="Order" type="number" min={0} error={errors.order?.message} {...register('order')} />
      </div>
      <Textarea label="Instructions" rows={4} error={errors.instructions?.message} {...register('instructions')} />
      <Textarea label="Expected outcome" rows={3} error={errors.expectedOutcome?.message} {...register('expectedOutcome')} />
      <Footer onClose={onClose} loading={isSubmitting} label="Add exercise" />
    </form>
  );
}

function AssessmentForm({
  moduleId,
  type,
  nextOrder,
  onClose,
  onSaved,
}: {
  moduleId: string;
  type: ActivityType;
  nextOrder: number;
  onClose: () => void;
  onSaved: () => void;
}) {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
  } = useForm<AssessmentFormValues>({
    resolver: zodResolver(assessmentSchema),
    defaultValues: {
      title: '',
      order: nextOrder,
      passingScore: 50,
      durationMinutes: undefined,
      questions: [
        {
          text: '',
          type: QuestionType.MultipleChoice,
          points: 1,
          answers: [
            { text: '', isCorrect: true },
            { text: '', isCorrect: false },
          ],
        },
      ],
    },
  });

  const onSubmit = async (v: AssessmentFormValues) => {
    const payload = {
      title: v.title,
      order: v.order,
      passingScore: v.passingScore,
      durationMinutes: v.durationMinutes || null,
      questions: v.questions.map((q) => ({
        text: q.text,
        type: q.type,
        points: q.points,
        answers: q.answers.map((a) => ({ text: a.text, isCorrect: a.isCorrect })),
      })),
    };
    try {
      if (type === ActivityType.Quiz) await activityService.createQuiz(moduleId, payload);
      else await activityService.createExam(moduleId, payload);
      notify.success(type === ActivityType.Quiz ? 'Quiz added.' : 'Exam added.');
      onSaved();
      onClose();
    } catch (err) {
      notify.apiError(err);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input label="Title" error={errors.title?.message} required {...register('title')} />
      <div className="grid gap-4 sm:grid-cols-3">
        <Input label="Order" type="number" min={0} error={errors.order?.message} {...register('order')} />
        <Input label="Passing score (%)" type="number" min={0} max={100} error={errors.passingScore?.message} {...register('passingScore')} />
        <Input label="Time limit (min)" type="number" min={0} placeholder="Optional" error={errors.durationMinutes?.message} {...register('durationMinutes')} />
      </div>
      <div className="border-t border-ink-100 pt-4">
        <QuestionsBuilder control={control} register={register} errors={errors} />
      </div>
      <Footer onClose={onClose} loading={isSubmitting} label={type === ActivityType.Quiz ? 'Add quiz' : 'Add exam'} />
    </form>
  );
}
