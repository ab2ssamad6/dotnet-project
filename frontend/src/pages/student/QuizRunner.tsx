import { useEffect, useMemo, useState } from 'react';
import { Button, Card, CardBody, Icons } from '@/components/ui';
import { enrollmentService } from '@/services';
import { notify } from '@/utils/toast';
import { useNotifications } from '@/features/notifications/NotificationsContext';
import { formatDuration } from '@/utils/format';
import { ActivityType, QuestionType, type ActivityDto, type QuizResultDto, type SubmittedAnswer } from '@/types';

interface Props {
  trainingId: string;
  activity: ActivityDto;
  onCompleted?: (result: QuizResultDto) => void;
}

/** Interactive quiz/exam runner with optional timer, scoring and result display. */
export function QuizRunner({ trainingId, activity, onCompleted }: Props) {
  const questions = activity.questions ?? [];
  const [answers, setAnswers] = useState<Record<string, string[]>>({});
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState<QuizResultDto | null>(null);
  const { push } = useNotifications();

  const totalSeconds = (activity.durationMinutes ?? 0) * 60;
  const [remaining, setRemaining] = useState(totalSeconds);

  const answeredCount = Object.values(answers).filter((a) => a.length > 0).length;

  const toggle = (questionId: string, answerId: string, type: QuestionType) => {
    setAnswers((prev) => {
      const current = prev[questionId] ?? [];
      if (type === QuestionType.MultipleAnswers) {
        return {
          ...prev,
          [questionId]: current.includes(answerId) ? current.filter((x) => x !== answerId) : [...current, answerId],
        };
      }
      // single-select (MultipleChoice / TrueFalse)
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

  // Countdown timer (auto-submits at zero).
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
      <Card>
        <CardBody className="text-center">
          <div
            className={`mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full ${
              result.passed ? 'bg-emerald-100 text-emerald-600' : 'bg-amber-100 text-amber-600'
            }`}
          >
            {result.passed ? <Icons.award size={32} /> : <Icons.refresh size={30} />}
          </div>
          <h3 className="text-xl font-bold text-slate-900">{result.passed ? 'Passed!' : 'Keep practicing'}</h3>
          <p className="mt-1 text-sm text-slate-500">
            You scored <span className="font-semibold text-slate-700">{result.score}%</span> —{' '}
            {result.correctCount}/{result.totalQuestions} correct.
          </p>
          <div className="mx-auto mt-4 max-w-xs">
            <div className="h-2.5 overflow-hidden rounded-full bg-slate-200">
              <div
                className={`h-full rounded-full ${result.passed ? 'bg-emerald-500' : 'bg-amber-500'}`}
                style={{ width: `${result.score}%` }}
              />
            </div>
          </div>
          {!result.passed && (
            <Button
              className="mt-5"
              variant="outline"
              onClick={() => {
                setResult(null);
                setAnswers({});
                setRemaining(totalSeconds);
              }}
            >
              Retry
            </Button>
          )}
        </CardBody>
      </Card>
    );
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between rounded-lg border border-slate-200 bg-white px-4 py-3">
        <div>
          <p className="text-sm font-semibold text-slate-800">
            {activity.type === ActivityType.Exam ? 'Exam' : 'Quiz'}: {activity.title}
          </p>
          <p className="text-xs text-slate-400">
            {answeredCount}/{questions.length} answered · pass mark {activity.passingScore}%
          </p>
        </div>
        {totalSeconds > 0 && (
          <div
            className={`flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm font-semibold ${
              remaining < 30 ? 'bg-rose-50 text-rose-600' : 'bg-slate-100 text-slate-600'
            }`}
          >
            <Icons.clock size={16} />
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
                <div className="mb-3 flex items-start justify-between gap-3">
                  <p className="font-medium text-slate-800">
                    <span className="mr-2 text-slate-400">{i + 1}.</span>
                    {q.text}
                  </p>
                  <span className="shrink-0 text-xs text-slate-400">
                    {q.points} pt{q.points > 1 ? 's' : ''}
                  </span>
                </div>
                {multi && <p className="mb-2 text-xs text-slate-400">Select all that apply.</p>}
                <div className="space-y-2">
                  {q.answers.map((a) => {
                    const selected = (answers[q.id] ?? []).includes(a.id);
                    return (
                      <button
                        key={a.id}
                        type="button"
                        onClick={() => toggle(q.id, a.id, q.type)}
                        className={`flex w-full items-center gap-3 rounded-lg border px-3 py-2.5 text-left text-sm transition-colors ${
                          selected ? 'border-brand-500 bg-brand-50 text-brand-800' : 'border-slate-200 hover:bg-slate-50'
                        }`}
                      >
                        <span
                          className={`flex h-5 w-5 shrink-0 items-center justify-center border ${
                            multi ? 'rounded' : 'rounded-full'
                          } ${selected ? 'border-brand-500 bg-brand-500 text-white' : 'border-slate-300'}`}
                        >
                          {selected && <Icons.check size={13} />}
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

      <div className="mt-5 flex items-center justify-between">
        <p className="text-sm text-slate-500">
          {activity.durationMinutes ? `Time limit: ${formatDuration(activity.durationMinutes)}` : 'No time limit'}
        </p>
        <Button loading={submitting} disabled={answeredCount === 0} onClick={submit}>
          Submit {activity.type === ActivityType.Exam ? 'exam' : 'quiz'}
        </Button>
      </div>
    </div>
  );
}
