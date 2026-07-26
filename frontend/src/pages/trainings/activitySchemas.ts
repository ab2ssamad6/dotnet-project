import { z } from 'zod';
import { QuestionType } from '@/types';

export const lessonSchema = z.object({
  title: z.string().min(1, 'Title is required.').max(200),
  order: z.coerce.number().int().min(0),
  content: z.string().optional(),
  videoUrl: z.string().url('Enter a valid URL.').optional().or(z.literal('')),
});
export type LessonFormValues = z.infer<typeof lessonSchema>;

export const exerciseSchema = z.object({
  title: z.string().min(1, 'Title is required.').max(200),
  order: z.coerce.number().int().min(0),
  instructions: z.string().optional(),
  expectedOutcome: z.string().optional(),
});
export type ExerciseFormValues = z.infer<typeof exerciseSchema>;

const answerSchema = z.object({
  text: z.string().min(1, 'Answer text is required.'),
  isCorrect: z.boolean(),
});

const questionSchema = z
  .object({
    text: z.string().min(1, 'Question text is required.'),
    type: z.coerce.number().int().min(0).max(2),
    points: z.coerce.number().int().min(1, 'Points must be at least 1.'),
    answers: z.array(answerSchema).min(2, 'Add at least two answers.'),
  })
  .refine((q) => q.answers.some((a) => a.isCorrect), {
    message: 'Mark at least one answer as correct.',
    path: ['answers'],
  })
  .refine((q) => q.type !== QuestionType.MultipleChoice || q.answers.filter((a) => a.isCorrect).length === 1, {
    message: 'Multiple-choice questions must have exactly one correct answer.',
    path: ['answers'],
  });

export const assessmentSchema = z.object({
  title: z.string().min(1, 'Title is required.').max(200),
  order: z.coerce.number().int().min(0),
  passingScore: z.coerce.number().int().min(0).max(100),
  durationMinutes: z.coerce.number().int().min(0).optional(),
  questions: z.array(questionSchema).min(1, 'Add at least one question.'),
});
export type AssessmentFormValues = z.infer<typeof assessmentSchema>;
