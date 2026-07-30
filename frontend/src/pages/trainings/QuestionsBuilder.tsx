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
      <p className="text-xs font-medium text-slate-500">Answers — tick the correct one(s)</p>
      {fields.map((field, aIndex) => (
        <div key={field.id} className="flex items-center gap-2">
          <input
            type="checkbox"
            className="h-4 w-4 shrink-0 rounded border-slate-300 accent-brand-600"
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
        <h4 className="text-sm font-semibold text-slate-700">Questions</h4>
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
        <p className="rounded-lg border border-dashed border-slate-200 py-6 text-center text-sm text-slate-400">
          No questions yet. Add at least one.
        </p>
      )}

      {typeof errors.questions?.message === 'string' && (
        <p className="text-xs text-rose-600">{errors.questions.message}</p>
      )}

      {fields.map((field, qIndex) => (
        <div key={field.id} className="rounded-xl border border-slate-200 bg-slate-50/60 p-4">
          <div className="mb-3 flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wide text-slate-400">Question {qIndex + 1}</span>
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
