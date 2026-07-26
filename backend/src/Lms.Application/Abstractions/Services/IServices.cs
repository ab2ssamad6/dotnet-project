using Lms.Application.Common;
using Lms.Application.Dtos.Activities;
using Lms.Application.Dtos.Auth;
using Lms.Application.Dtos.Categories;
using Lms.Application.Dtos.Dashboard;
using Lms.Application.Dtos.Enrollments;
using Lms.Application.Dtos.Modules;
using Lms.Application.Dtos.Trainers;
using Lms.Application.Dtos.Trainings;

namespace Lms.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task<Result> LogoutAsync(LogoutRequest request, CancellationToken ct = default);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<Result> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
}

public interface ICategoryService
{
    Task<Result<PagedResult<CategoryDto>>> GetPagedAsync(PagedQuery query, CancellationToken ct = default);
    Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface ITrainerService
{
    Task<Result<PagedResult<TrainerDto>>> GetPagedAsync(PagedQuery query, CancellationToken ct = default);
    Task<Result<TrainerDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<TrainerDto>> CreateAsync(CreateTrainerRequest request, CancellationToken ct = default);
    Task<Result<TrainerDto>> UpdateAsync(Guid id, UpdateTrainerRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface ITrainingService
{
    Task<Result<PagedResult<TrainingDto>>> GetPagedAsync(PagedQuery query, bool onlyPublished, CancellationToken ct = default);
    Task<Result<TrainingDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<TrainingDto>> CreateAsync(CreateTrainingRequest request, CancellationToken ct = default);
    Task<Result<TrainingDto>> UpdateAsync(Guid id, UpdateTrainingRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IModuleService
{
    Task<Result<IReadOnlyList<ModuleDto>>> GetByTrainingAsync(Guid trainingId, CancellationToken ct = default);
    Task<Result<ModuleDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ModuleDto>> CreateAsync(CreateModuleRequest request, CancellationToken ct = default);
    Task<Result<ModuleDto>> UpdateAsync(Guid id, UpdateModuleRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IActivityService
{
    Task<Result<IReadOnlyList<ActivityDto>>> GetByModuleAsync(Guid moduleId, bool includeCorrectAnswers, CancellationToken ct = default);
    Task<Result<ActivityDto>> GetByIdAsync(Guid id, bool includeCorrectAnswers, CancellationToken ct = default);
    Task<Result<ActivityDto>> CreateLessonAsync(Guid moduleId, CreateLessonRequest request, CancellationToken ct = default);
    Task<Result<ActivityDto>> CreateExerciseAsync(Guid moduleId, CreateExerciseRequest request, CancellationToken ct = default);
    Task<Result<ActivityDto>> CreateQuizAsync(Guid moduleId, CreateQuizRequest request, CancellationToken ct = default);
    Task<Result<ActivityDto>> CreateExamAsync(Guid moduleId, CreateExamRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IEnrollmentService
{
    Task<Result<EnrollmentDto>> EnrollAsync(Guid studentId, EnrollRequest request, CancellationToken ct = default);
    Task<Result<IReadOnlyList<EnrollmentDto>>> GetMyEnrollmentsAsync(Guid studentId, CancellationToken ct = default);
    Task<Result<ProgressDto>> GetProgressAsync(Guid studentId, Guid trainingId, CancellationToken ct = default);
    Task<Result<ProgressDto>> CompleteModuleAsync(Guid studentId, Guid trainingId, CompleteModuleRequest request, CancellationToken ct = default);
    Task<Result<QuizResultDto>> SubmitQuizAsync(Guid studentId, Guid trainingId, SubmitQuizRequest request, CancellationToken ct = default);
    Task<Result<CertificateDto>> GetCertificateAsync(Guid studentId, Guid trainingId, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<Result<DashboardDto>> GetAsync(CancellationToken ct = default);
}
