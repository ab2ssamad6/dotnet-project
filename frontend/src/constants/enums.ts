import {
  ActivityType,
  DifficultyLevel,
  EnrollmentStatus,
  QuestionType,
  TrainingStatus,
} from '@/types';

export interface EnumOption<T> {
  value: T;
  label: string;
  badge: string;
}

export const DIFFICULTY_OPTIONS: EnumOption<DifficultyLevel>[] = [
  { value: DifficultyLevel.Beginner, label: 'Beginner', badge: 'bg-emerald-100 text-emerald-700' },
  { value: DifficultyLevel.Intermediate, label: 'Intermediate', badge: 'bg-sky-100 text-sky-700' },
  { value: DifficultyLevel.Advanced, label: 'Advanced', badge: 'bg-amber-100 text-amber-700' },
  { value: DifficultyLevel.Expert, label: 'Expert', badge: 'bg-rose-100 text-rose-700' },
];

export const TRAINING_STATUS_OPTIONS: EnumOption<TrainingStatus>[] = [
  { value: TrainingStatus.Draft, label: 'Draft', badge: 'bg-slate-100 text-slate-600' },
  { value: TrainingStatus.Published, label: 'Published', badge: 'bg-emerald-100 text-emerald-700' },
  { value: TrainingStatus.Archived, label: 'Archived', badge: 'bg-slate-200 text-slate-500' },
];

export const ENROLLMENT_STATUS_OPTIONS: EnumOption<EnrollmentStatus>[] = [
  { value: EnrollmentStatus.Active, label: 'Active', badge: 'bg-sky-100 text-sky-700' },
  { value: EnrollmentStatus.Completed, label: 'Completed', badge: 'bg-emerald-100 text-emerald-700' },
  { value: EnrollmentStatus.Cancelled, label: 'Cancelled', badge: 'bg-rose-100 text-rose-700' },
];

export const ACTIVITY_TYPE_OPTIONS: EnumOption<ActivityType>[] = [
  { value: ActivityType.Lesson, label: 'Lesson', badge: 'bg-sky-100 text-sky-700' },
  { value: ActivityType.Exercise, label: 'Exercise', badge: 'bg-violet-100 text-violet-700' },
  { value: ActivityType.Quiz, label: 'Quiz', badge: 'bg-amber-100 text-amber-700' },
  { value: ActivityType.Exam, label: 'Exam', badge: 'bg-rose-100 text-rose-700' },
];

export const QUESTION_TYPE_OPTIONS: EnumOption<QuestionType>[] = [
  { value: QuestionType.MultipleChoice, label: 'Multiple Choice (single answer)', badge: 'bg-sky-100 text-sky-700' },
  { value: QuestionType.MultipleAnswers, label: 'Multiple Answers', badge: 'bg-violet-100 text-violet-700' },
  { value: QuestionType.TrueFalse, label: 'True / False', badge: 'bg-slate-100 text-slate-700' },
];

function lookup<T>(options: EnumOption<T>[], value: T): EnumOption<T> {
  return options.find((o) => o.value === value) ?? { value, label: String(value), badge: 'bg-slate-100 text-slate-600' };
}

export const difficultyOption = (v: DifficultyLevel) => lookup(DIFFICULTY_OPTIONS, v);
export const trainingStatusOption = (v: TrainingStatus) => lookup(TRAINING_STATUS_OPTIONS, v);
export const enrollmentStatusOption = (v: EnrollmentStatus) => lookup(ENROLLMENT_STATUS_OPTIONS, v);
export const activityTypeOption = (v: ActivityType) => lookup(ACTIVITY_TYPE_OPTIONS, v);
export const questionTypeOption = (v: QuestionType) => lookup(QUESTION_TYPE_OPTIONS, v);
