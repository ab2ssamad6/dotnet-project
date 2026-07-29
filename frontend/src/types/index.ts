/**
 * Domain types mirroring the LMS .NET API DTOs.
 * NOTE: The API serializes enums as **numbers** (no JsonStringEnumConverter),
 * so every enum below uses numeric values matching Lms.Domain/Enums/Enums.cs.
 */

// ---------- Enums ----------
export enum Role {
  Administrator = 'Administrator',
  Trainer = 'Trainer',
  Student = 'Student',
}

export enum DifficultyLevel {
  Beginner = 0,
  Intermediate = 1,
  Advanced = 2,
  Expert = 3,
}

export enum TrainingStatus {
  Draft = 0,
  Published = 1,
  Archived = 2,
}

export enum ActivityType {
  Lesson = 0,
  Exercise = 1,
  Quiz = 2,
  Exam = 3,
}

export enum QuestionType {
  MultipleChoice = 0,
  MultipleAnswers = 1,
  TrueFalse = 2,
}

export enum EnrollmentStatus {
  Active = 0,
  Completed = 1,
  Cancelled = 2,
}

// ---------- Common ----------
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface PagedQuery {
  page?: number;
  pageSize?: number;
  search?: string;
}

/** RFC7807 Problem Details returned by the API on failure. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

// ---------- Auth ----------
export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  emailConfirmed: boolean;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  role?: string | null;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface VerifyEmailRequest {
  userId: string;
  token: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

// ---------- Categories ----------
export interface CategoryDto {
  id: string;
  name: string;
  description?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CategoryRequest {
  name: string;
  description?: string | null;
}

// ---------- Trainings ----------
export interface TrainingDto {
  id: string;
  title: string;
  description: string;
  difficulty: DifficultyLevel;
  duration: number;
  thumbnail?: string | null;
  status: TrainingStatus;
  published: boolean;
  categoryId: string;
  categoryName?: string | null;
  trainerId: string;
  trainerName?: string | null;
  moduleCount: number;
  createdAt: string;
  updatedAt?: string | null;
}

export interface TrainingRequest {
  title: string;
  description: string;
  difficulty: DifficultyLevel;
  duration: number;
  thumbnail?: string | null;
  status: TrainingStatus;
  published: boolean;
  categoryId: string;
  trainerId: string;
}

// ---------- Modules ----------
export interface ModuleDto {
  id: string;
  trainingId: string;
  title: string;
  description?: string | null;
  order: number;
  duration: number;
  videoUrl?: string | null;
  attachment?: string | null;
  aiAvatarEnabled: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateModuleRequest {
  trainingId: string;
  title: string;
  description?: string | null;
  order: number;
  duration: number;
  videoUrl?: string | null;
  attachment?: string | null;
  aiAvatarEnabled: boolean;
}

export type UpdateModuleRequest = Omit<CreateModuleRequest, 'trainingId'>;

// ---------- Activities ----------
export interface AnswerDto {
  id: string;
  text: string;
  isCorrect?: boolean | null;
}

export interface QuestionDto {
  id: string;
  text: string;
  type: QuestionType;
  points: number;
  answers: AnswerDto[];
}

export interface ActivityDto {
  id: string;
  moduleId: string;
  type: ActivityType;
  title: string;
  order: number;
  content?: string | null;
  videoUrl?: string | null;
  instructions?: string | null;
  expectedOutcome?: string | null;
  passingScore?: number | null;
  durationMinutes?: number | null;
  questions?: QuestionDto[] | null;
}

export interface CreateLessonRequest {
  title: string;
  order: number;
  content?: string | null;
  videoUrl?: string | null;
}

export interface CreateExerciseRequest {
  title: string;
  order: number;
  instructions?: string | null;
  expectedOutcome?: string | null;
}

export interface CreateAnswerRequest {
  text: string;
  isCorrect: boolean;
}

export interface CreateQuestionRequest {
  text: string;
  type: QuestionType;
  points: number;
  answers: CreateAnswerRequest[];
}

export interface CreateAssessmentRequest {
  title: string;
  order: number;
  passingScore: number;
  durationMinutes?: number | null;
  questions: CreateQuestionRequest[];
}

// ---------- Trainers ----------
export interface TrainerDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  biography?: string | null;
  avatar?: string | null;
  expertise?: string | null;
  phone?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface TrainerRequest {
  firstName: string;
  lastName: string;
  email: string;
  biography?: string | null;
  avatar?: string | null;
  expertise?: string | null;
  phone?: string | null;
}

// ---------- Enrollments ----------
export interface EnrollmentDto {
  id: string;
  studentId: string;
  trainingId: string;
  trainingTitle?: string | null;
  enrolledAt: string;
  progressPercent: number;
  status: EnrollmentStatus;
  completedAt?: string | null;
}

export interface ModuleProgressDto {
  moduleId: string;
  title: string;
  order: number;
  completed: boolean;
  completedAt?: string | null;
}

export interface ProgressDto {
  enrollmentId: string;
  trainingId: string;
  trainingTitle?: string | null;
  progressPercent: number;
  status: EnrollmentStatus;
  modules: ModuleProgressDto[];
}

export interface SubmittedAnswer {
  questionId: string;
  selectedAnswerIds: string[];
}

export interface SubmitQuizRequest {
  activityId: string;
  answers: SubmittedAnswer[];
}

export interface QuizResultDto {
  activityId: string;
  score: number;
  passed: boolean;
  correctCount: number;
  totalQuestions: number;
  submittedAt: string;
}

export interface CertificateDto {
  enrollmentId: string;
  trainingId: string;
  trainingTitle?: string | null;
  studentName: string;
  available: boolean;
  issuedAt?: string | null;
  message?: string | null;
}

// ---------- Dashboard ----------
export interface DashboardCountsDto {
  students: number;
  trainers: number;
  courses: number;
  modules: number;
  enrollments: number;
  publishedCourses: number;
}

export interface RecentActivityDto {
  type: string;
  description: string;
  timestamp: string;
}

export interface CategoryCountDto {
  category: string;
  trainings: number;
}

export interface DashboardDto {
  counts: DashboardCountsDto;
  trainingsByCategory: CategoryCountDto[];
  recentActivity: RecentActivityDto[];
}

// ---------- AI Trainer ----------
export interface StartSessionRequest {
  moduleId?: string | null;
  personaName?: string | null;
  /** Scopes the avatar to a training so it tutors that subject. Implied when moduleId is set. */
  trainingId?: string | null;
}

export interface StartSessionResponse {
  sessionToken: string;
  provider: string;
  moduleId?: string | null;
  issuedAt: string;
  trainingId?: string | null;
  /** Title of the subject the persona was primed with, for labelling the session. */
  subjectTitle?: string | null;
  personaName?: string | null;
}

export interface AskQuestionRequest {
  sessionToken: string;
  question: string;
  moduleId?: string | null;
  trainingId?: string | null;
}

export interface AskQuestionResponse {
  answer: string;
  live: boolean;
}

export interface ModulePresentationResponse {
  moduleId: string;
  presentation: string;
  live: boolean;
}
