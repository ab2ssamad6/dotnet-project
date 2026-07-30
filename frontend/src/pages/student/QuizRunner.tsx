import { useEffect, useMemo, useState } from 'react';
import { Button, Card, CardBody, Icons } from '@/components/ui';
import { enrollmentService } from '@/services';
import { notify } from '@/utils/toast';
import { useNotifications } from '@/features/notifications/NotificationsContext';
import { formatDuration } from '@/utils/format';
import { cn } from '@/utils/cn';
import { ActivityType, QuestionType, type ActivityDto, type QuizResultDto, type SubmittedAnswer } from '@/types';

interface Props {
  trainingId: string;
  activity: ActivityDto;
  onCompleted?: (result: QuizResultDto) => void;
}

export function QuizRunner({ trainingId, activity, onCompleted }: Props) {
  const questions = activity.questions ?? [];
  const [answers, setAnswers] = useState<Record<string, string[]>>({});
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState<QuizResultDto | null>(null);
  const { push } = useNotifications();

  const totalSeconds = (activity.durationMinutes ?? 0) * 60;
  const [remaining, setRemaining] = useState(totalSeconds);

  const answeredCount = Object.values(answers).filter((a) => a.length > 0).length;
  const label = activity.type === ActivityType.Exam ? 'Exam' : 'Quiz';

  const toggle = (questionId: string, answerId: string, type: QuestionType) => {
    setAnswers((prev) => {
      const current = prev[questionId] ?? [];
      if (type === QuestionType.MultipleAnswers) {
        return {
          ...prev,
          [questionId]: current.includes(answerId) ? current.filter((x) => x !== answerId) : [...current, answerId],
        };
      }
      return { ...prev, [questionId]: [answerId] };
    });
  };

  const submit = useMemo(
    () => async () => {
      setSubmitting(true);
      try {
        const payload = {
          activityId: activity.id,
          answers: questions.map<SubmittedAnswer>((q) => ({
            questionId: q.id,
            selectedAnswerIds: answers[q.id] ?? [],
          })),
        };
        const res = await enrollmentService.submitQuiz(trainingId, payload);
        setResult(res);
        push({
          title: res.passed ? 'Assessment passed' : 'Assessment completed',
          message: `${activity.title}: scored ${res.score}%`,
          type: res.passed ? 'success' : 'warning',
        });
        onCompleted?.(res);
      } catch (err) {
        notify.apiError(err);
      } finally {
        setSubmitting(false);
      }
    },
    [activity.id, activity.title, answers, onCompleted, push, questions, trainingId],
  );

  useEffect(() => {
    if (!totalSeconds || result) return;
    if (remaining <= 0) {
      void submit();
      return;
    }
    const t = setTimeout(() => setRemaining((r) => r - 1), 1000);
    return () => clearTimeout(t);
  }, [remaining, totalSeconds, result, submit]);

  if (result) {
    return (
      <Card className={result.passed ? 'border-green-200' : 'border-gold-200'}>
        <CardBody className="py-8 text-center">
          <div
            className={cn(
              'mx-auto mb-5 flex h-16 w-16 items-center justify-center rounded-2xl shadow-card ring-1 ring-inset',
              result.passed
                ? 'bg-green-50 text-green-600 ring-green-200/70'
                : 'bg-gold-50 text-gold-600 ring-gold-200/70',
            )}
          >
            {result.passed ? <Icons.award size={30} /> : <Icons.refresh size={28} />}
          </div>
          <h3 className="font-display text-[24px] font-semibold tracking-[-0.02em] text-ink-900">
            {result.passed ? 'You passed' : 'Not quite yet'}
          </h3>
          <p className="mt-2 text-sm text-ink-500">
            You scored <span className="tnum font-bold text-ink-800">{result.score}%</span> —{' '}
            <span className="tnum">
              {result.correctCount}/{result.totalQuestions}
            </span>{' '}
            correct.
          </p>
          <div className="mx-auto mt-5 max-w-xs">
            <div className="h-2.5 overflow-hidden rounded-full bg-ink-200/80">
              <div
                className={cn(
                  'h-full rounded-full transition-[width] duration-700 ease-out',
                  result.passed ? 'bg-green-500' : 'bg-gold-400',
                )}
                style={{ width: `${result.score}%` }}
              />
            </div>
          </div>
          {!result.passed && (
            <Button
              className="mt-6"
              variant="outline"
              leftIcon={<Icons.refresh size={15} />}
              onClick={() => {
                setResult(null);
                setAnswers({});
                setRemaining(totalSeconds);
              }}
            >
              Try again
            </Button>
          )}
        </CardBody>
      </Card>
    );
  }

  return (
    <div>
      <div className="surface mb-4 flex items-center justify-between gap-4 px-4 py-3.5">
        <div className="min-w-0">
          <p className="truncate text-sm font-bold text-ink-900">
            {label}: {activity.title}
          </p>
          <p className="tnum mt-0.5 text-[11.5px] font-medium text-ink-400">
            {answeredCount}/{questions.length} answered · pass mark {activity.passingScore}%
          </p>
        </div>
        {totalSeconds > 0 && (
          <div
            className={cn(
              'tnum flex shrink-0 items-center gap-1.5 rounded-lg px-3 py-2 text-sm font-bold ring-1 ring-inset',
              remaining < 30
                ? 'bg-rose-50 text-rose-600 ring-rose-200/70'
                : 'bg-ink-100 text-ink-700 ring-ink-200/70',
            )}
          >
            <Icons.clock size={15} />
            {String(Math.floor(remaining / 60)).padStart(2, '0')}:{String(remaining % 60).padStart(2, '0')}
          </div>
        )}
      </div>

      <div className="space-y-4">
        {questions.map((q, i) => {
          const multi = q.type === QuestionType.MultipleAnswers;
          return (
            <Card key={q.id}>
              <CardBody>
                <div className="mb-4 flex items-start justify-between gap-4">
                  <p className="text-[14.5px] font-semibold leading-relaxed text-ink-800">
                    <span className="tnum mr-2 font-bold text-ink-300">{String(i + 1).padStart(2, '0')}</span>
                    {q.text}
                  </p>
                  <span className="tnum shrink-0 rounded-md bg-ink-100 px-2 py-1 text-[11px] font-bold text-ink-500">
                    {q.points} pt{q.points > 1 ? 's' : ''}
                  </span>
                </div>
                {multi && <p className="mb-2.5 text-[11.5px] font-semibold text-ink-400">Select all that apply.</p>}
                <div className="space-y-2">
                  {q.answers.map((a) => {
                    const selected = (answers[q.id] ?? []).includes(a.id);
                    return (
                      <button
                        key={a.id}
                        type="button"
                        onClick={() => toggle(q.id, a.id, q.type)}
                        className={cn(
                          'focus-ring flex w-full items-center gap-3 rounded-xl border px-3.5 py-3 text-left text-[13.5px] transition-all',
                          selected
                            ? 'border-brand-400 bg-brand-50 font-semibold text-brand-900 shadow-card'
                            : 'border-ink-200/80 text-ink-700 hover:border-ink-300 hover:bg-ink-50/70',
                        )}
                      >
                        <span
                          className={cn(
                            'flex h-5 w-5 shrink-0 items-center justify-center border transition-colors',
                            multi ? 'rounded-[6px]' : 'rounded-full',
                            selected ? 'border-brand-600 bg-brand-600 text-white' : 'border-ink-300 bg-white',
                          )}
                        >
                          {selected && <Icons.check size={12} />}
                        </span>
                        {a.text}
                      </button>
                    );
                  })}
                </div>
              </CardBody>
            </Card>
          );
        })}
      </div>

      <div className="mt-5 flex flex-wrap items-center justify-between gap-3">
        <p className="text-[13px] text-ink-500">
          {activity.durationMinutes ? `Time limit: ${formatDuration(activity.durationMinutes)}` : 'No time limit'}
        </p>
        <Button loading={submitting} disabled={answeredCount === 0} onClick={submit}>
          Submit {label.toLowerCase()}
        </Button>
      </div>
    </div>
  );
}
