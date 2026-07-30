import { useFieldArray, type Control, type UseFormRegister, type FieldErrors } from 'react-hook-form';
import { Button, Icons, Input, Select } from '@/components/ui';
import { QUESTION_TYPE_OPTIONS } from '@/constants/enums';
import { QuestionType } from '@/types';
import type { AssessmentFormValues } from './activitySchemas';

function AnswersEditor({
  control,
  register,
  qIndex,
  errors,
}: {
  control: Control<AssessmentFormValues>;
  register: UseFormRegister<AssessmentFormValues>;
  qIndex: number;
  errors: FieldErrors<AssessmentFormValues>;
}) {
  const { fields, append, remove } = useFieldArray({ control, name: `questions.${qIndex}.answers` });
  const answerError = errors.questions?.[qIndex]?.answers as { message?: string } | undefined;

  return (
    <div className="space-y-2">
      <p className="eyebrow">Answers — tick the correct one(s)</p>
      {fields.map((field, aIndex) => (
        <div key={field.id} className="flex items-center gap-2">
          <input
            type="checkbox"
            className="focus-ring h-[18px] w-[18px] shrink-0 cursor-pointer rounded-[5px] border-ink-300 accent-brand-700"
            aria-label="Correct answer"
            {...register(`questions.${qIndex}.answers.${aIndex}.isCorrect`)}
          />
          <Input
            className="flex-1"
            placeholder={`Answer ${aIndex + 1}`}
            {...register(`questions.${qIndex}.answers.${aIndex}.text`)}
          />
          <Button
            type="button"
            variant="ghost"
            size="icon"
            className="text-rose-500 hover:bg-rose-50"
            onClick={() => remove(aIndex)}
            disabled={fields.length <= 2}
            aria-label="Remove answer"
          >
            <Icons.close size={16} />
          </Button>
        </div>
      ))}
      {answerError?.message && <p className="text-xs text-rose-600">{answerError.message}</p>}
      <Button type="button" variant="ghost" size="sm" leftIcon={<Icons.plus size={15} />} onClick={() => append({ text: '', isCorrect: false })}>
        Add answer
      </Button>
    </div>
  );
}

export function QuestionsBuilder({
  control,
  register,
  errors,
}: {
  control: Control<AssessmentFormValues>;
  register: UseFormRegister<AssessmentFormValues>;
  errors: FieldErrors<AssessmentFormValues>;
}) {
  const { fields, append, remove } = useFieldArray({ control, name: 'questions' });

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h4 className="text-sm font-bold tracking-[-0.01em] text-ink-800">Questions</h4>
        <Button
          type="button"
          variant="outline"
          size="sm"
          leftIcon={<Icons.plus size={15} />}
          onClick={() =>
            append({
              text: '',
              type: QuestionType.MultipleChoice,
              points: 1,
              answers: [
                { text: '', isCorrect: true },
                { text: '', isCorrect: false },
              ],
            })
          }
        >
          Add question
        </Button>
      </div>

      {fields.length === 0 && (
        <p className="rounded-xl border border-dashed border-ink-300/80 py-7 text-center text-[13px] text-ink-400">
          No questions yet — add at least one to score this assessment.
        </p>
      )}

      {typeof errors.questions?.message === 'string' && (
        <p className="text-xs text-rose-600">{errors.questions.message}</p>
      )}

      {fields.map((field, qIndex) => (
        <div key={field.id} className="rounded-2xl border border-ink-200/80 bg-ink-50/70 p-4">
          <div className="mb-3 flex items-center justify-between">
            <span className="eyebrow">Question {qIndex + 1}</span>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              className="text-rose-500 hover:bg-rose-50"
              onClick={() => remove(qIndex)}
              aria-label="Remove question"
            >
              <Icons.trash size={16} />
            </Button>
          </div>
          <div className="space-y-3">
            <Input
              label="Question text"
              placeholder="Enter the question…"
              error={errors.questions?.[qIndex]?.text?.message}
              {...register(`questions.${qIndex}.text`)}
            />
            <div className="grid gap-3 sm:grid-cols-2">
              <Select
                label="Type"
                options={QUESTION_TYPE_OPTIONS.map((o) => ({ value: o.value, label: o.label }))}
                {...register(`questions.${qIndex}.type`)}
              />
              <Input
                label="Points"
                type="number"
                min={1}
                error={errors.questions?.[qIndex]?.points?.message}
                {...register(`questions.${qIndex}.points`)}
              />
            </div>
            <AnswersEditor control={control} register={register} qIndex={qIndex} errors={errors} />
          </div>
        </div>
      ))}
    </div>
  );
}
