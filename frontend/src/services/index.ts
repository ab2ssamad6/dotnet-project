import { http } from '@/api/client';
import type {
  ActivityDto,
  AskQuestionRequest,
  AskQuestionResponse,
  CategoryDto,
  CategoryRequest,
  CertificateDto,
  CreateAssessmentRequest,
  CreateExerciseRequest,
  CreateLessonRequest,
  CreateModuleRequest,
  DashboardDto,
  EnrollmentDto,
  ModuleDto,
  ModulePresentationResponse,
  PagedQuery,
  PagedResult,
  ProgressDto,
  QuizResultDto,
  StartSessionRequest,
  StartSessionResponse,
  SubmitQuizRequest,
  TrainerDto,
  TrainerRequest,
  TrainingDto,
  TrainingRequest,
  UpdateModuleRequest,
} from '@/types';

function pagedParams(q: PagedQuery = {}) {
  return {
    page: q.page ?? 1,
    pageSize: q.pageSize ?? 10,
    ...(q.search ? { search: q.search } : {}),
  };
}

export const categoryService = {
  list: (q?: PagedQuery) => http.get<PagedResult<CategoryDto>>('/api/categories', { params: pagedParams(q) }),
  get: (id: string) => http.get<CategoryDto>(`/api/categories/${id}`),
  create: (body: CategoryRequest) => http.post<CategoryDto>('/api/categories', body),
  update: (id: string, body: CategoryRequest) => http.put<void>(`/api/categories/${id}`, body),
  remove: (id: string) => http.delete(`/api/categories/${id}`),
};

export const trainingService = {
  list: (q?: PagedQuery) => http.get<PagedResult<TrainingDto>>('/api/trainings', { params: pagedParams(q) }),
  catalog: (q?: PagedQuery) => http.get<PagedResult<TrainingDto>>('/api/catalog', { params: pagedParams(q) }),
  get: (id: string) => http.get<TrainingDto>(`/api/trainings/${id}`),
  create: (body: TrainingRequest) => http.post<TrainingDto>('/api/trainings', body),
  update: (id: string, body: TrainingRequest) => http.put<void>(`/api/trainings/${id}`, body),
  remove: (id: string) => http.delete(`/api/trainings/${id}`),
};

export const moduleService = {
  listByTraining: (trainingId: string) => http.get<ModuleDto[]>(`/api/trainings/${trainingId}/modules`),
  get: (id: string) => http.get<ModuleDto>(`/api/modules/${id}`),
  create: (body: CreateModuleRequest) => http.post<ModuleDto>('/api/modules', body),
  update: (id: string, body: UpdateModuleRequest) => http.put<void>(`/api/modules/${id}`, body),
  remove: (id: string) => http.delete(`/api/modules/${id}`),
};

export const activityService = {
  listByModule: (moduleId: string) => http.get<ActivityDto[]>(`/api/modules/${moduleId}/activities`),
  get: (id: string) => http.get<ActivityDto>(`/api/activities/${id}`),
  createLesson: (moduleId: string, body: CreateLessonRequest) =>
    http.post<ActivityDto>(`/api/modules/${moduleId}/lessons`, body),
  createExercise: (moduleId: string, body: CreateExerciseRequest) =>
    http.post<ActivityDto>(`/api/modules/${moduleId}/exercises`, body),
  createQuiz: (moduleId: string, body: CreateAssessmentRequest) =>
    http.post<ActivityDto>(`/api/modules/${moduleId}/quizzes`, body),
  createExam: (moduleId: string, body: CreateAssessmentRequest) =>
    http.post<ActivityDto>(`/api/modules/${moduleId}/exams`, body),
  remove: (id: string) => http.delete(`/api/activities/${id}`),
};

export const trainerService = {
  list: (q?: PagedQuery) => http.get<PagedResult<TrainerDto>>('/api/trainers', { params: pagedParams(q) }),
  get: (id: string) => http.get<TrainerDto>(`/api/trainers/${id}`),
  create: (body: TrainerRequest) => http.post<TrainerDto>('/api/trainers', body),
  update: (id: string, body: TrainerRequest) => http.put<void>(`/api/trainers/${id}`, body),
  remove: (id: string) => http.delete(`/api/trainers/${id}`),
};

export const enrollmentService = {
  enroll: (trainingId: string) => http.post<EnrollmentDto>('/api/enrollments', { trainingId }),
  myEnrollments: () => http.get<EnrollmentDto[]>('/api/enrollments'),
  progress: (trainingId: string) => http.get<ProgressDto>(`/api/enrollments/${trainingId}/progress`),
  completeModule: (trainingId: string, moduleId: string) =>
    http.post<void>(`/api/enrollments/${trainingId}/complete-module`, { moduleId }),
  submitQuiz: (trainingId: string, body: SubmitQuizRequest) =>
    http.post<QuizResultDto>(`/api/enrollments/${trainingId}/submit-quiz`, body),
  certificate: (trainingId: string) => http.get<CertificateDto>(`/api/enrollments/${trainingId}/certificate`),
};

export const dashboardService = {
  get: () => http.get<DashboardDto>('/api/admin/dashboard'),
};

export const aiTrainerService = {
  startSession: (body: StartSessionRequest) => http.post<StartSessionResponse>('/api/ai-trainer/session', body),
  ask: (body: AskQuestionRequest) => http.post<AskQuestionResponse>('/api/ai-trainer/ask', body),
  modulePresentation: (moduleId: string) =>
    http.post<ModulePresentationResponse>('/api/ai-trainer/module-presentation', { moduleId }),
  stopSession: (sessionToken: string) => http.post<void>('/api/ai-trainer/stop', { sessionToken }),
};

export { authService } from './authService';
