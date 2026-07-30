import type { BadgeTone } from '@/components/ui';
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
  tone: BadgeTone;
}

export const DIFFICULTY_OPTIONS: EnumOption<DifficultyLevel>[] = [
  { value: DifficultyLevel.Beginner, label: 'Beginner', tone: 'success' },
  { value: DifficultyLevel.Intermediate, label: 'Intermediate', tone: 'info' },
  { value: DifficultyLevel.Advanced, label: 'Advanced', tone: 'warning' },
  { value: DifficultyLevel.Expert, label: 'Expert', tone: 'danger' },
];

export const TRAINING_STATUS_OPTIONS: EnumOption<TrainingStatus>[] = [
  { value: TrainingStatus.Draft, label: 'Draft', tone: 'neutral' },
  { value: TrainingStatus.Published, label: 'Published', tone: 'success' },
  { value: TrainingStatus.Archived, label: 'Archived', tone: 'neutral' },
];

export const ENROLLMENT_STATUS_OPTIONS: EnumOption<EnrollmentStatus>[] = [
  { value: EnrollmentStatus.Active, label: 'In progress', tone: 'info' },
  { value: EnrollmentStatus.Completed, label: 'Completed', tone: 'success' },
  { value: EnrollmentStatus.Cancelled, label: 'Cancelled', tone: 'danger' },
];

export const ACTIVITY_TYPE_OPTIONS: EnumOption<ActivityType>[] = [
  { value: ActivityType.Lesson, label: 'Lesson', tone: 'info' },
  { value: ActivityType.Exercise, label: 'Exercise', tone: 'ai' },
  { value: ActivityType.Quiz, label: 'Quiz', tone: 'gold' },
  { value: ActivityType.Exam, label: 'Exam', tone: 'danger' },
];

export const QUESTION_TYPE_OPTIONS: EnumOption<QuestionType>[] = [
  { value: QuestionType.MultipleChoice, label: 'Multiple choice — one answer', tone: 'info' },
  { value: QuestionType.MultipleAnswers, label: 'Multiple choice — several answers', tone: 'ai' },
  { value: QuestionType.TrueFalse, label: 'True / False', tone: 'neutral' },
];

function lookup<T>(options: EnumOption<T>[], value: T): EnumOption<T> {
  return options.find((o) => o.value === value) ?? { value, label: String(value), tone: 'neutral' };
}

export const difficultyOption = (v: DifficultyLevel) => lookup(DIFFICULTY_OPTIONS, v);
export const trainingStatusOption = (v: TrainingStatus) => lookup(TRAINING_STATUS_OPTIONS, v);
export const enrollmentStatusOption = (v: EnrollmentStatus) => lookup(ENROLLMENT_STATUS_OPTIONS, v);
export const activityTypeOption = (v: ActivityType) => lookup(ACTIVITY_TYPE_OPTIONS, v);
export const questionTypeOption = (v: QuestionType) => lookup(QUESTION_TYPE_OPTIONS, v);
