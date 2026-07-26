using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Enrollments;
using Lms.Domain.Entities;
using Lms.Domain.Enums;
using Lms.Infrastructure.Identity;
using Lms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lms.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly LmsDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EnrollmentService(LmsDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<EnrollmentDto>> EnrollAsync(Guid studentId, EnrollRequest request, CancellationToken ct = default)
    {
        var training = await _context.Trainings.FirstOrDefaultAsync(t => t.Id == request.TrainingId, ct);
        if (training is null)
            return Result<EnrollmentDto>.NotFound("Training not found.");
        if (!training.Published)
            return Result<EnrollmentDto>.Validation("This training is not open for enrollment.");
        if (await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.TrainingId == training.Id, ct))
            return Result<EnrollmentDto>.Conflict("You are already enrolled in this training.");

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            TrainingId = training.Id,
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.Active,
            ProgressPercent = 0
        };
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(ct);
        return Result<EnrollmentDto>.Success(ToDto(enrollment, training.Title));
    }

    public async Task<Result<IReadOnlyList<EnrollmentDto>>> GetMyEnrollmentsAsync(Guid studentId, CancellationToken ct = default)
    {
        var list = await _context.Enrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new EnrollmentDto(e.Id, e.StudentId, e.TrainingId, e.Training!.Title,
                e.EnrolledAt, e.ProgressPercent, e.Status, e.CompletedAt))
            .ToListAsync(ct);
        return Result<IReadOnlyList<EnrollmentDto>>.Success(list);
    }

    public async Task<Result<ProgressDto>> GetProgressAsync(Guid studentId, Guid trainingId, CancellationToken ct = default)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Training)
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.TrainingId == trainingId, ct);
        if (enrollment is null)
            return Result<ProgressDto>.NotFound("You are not enrolled in this training.");

        return Result<ProgressDto>.Success(await BuildProgressAsync(enrollment, ct));
    }

    public async Task<Result<ProgressDto>> CompleteModuleAsync(Guid studentId, Guid trainingId, CompleteModuleRequest request, CancellationToken ct = default)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Training)
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.TrainingId == trainingId, ct);
        if (enrollment is null)
            return Result<ProgressDto>.NotFound("You are not enrolled in this training.");

        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Id == request.ModuleId, ct);
        if (module is null || module.TrainingId != trainingId)
            return Result<ProgressDto>.Validation("The module does not belong to this training.");

        var alreadyCompleted = await _context.ModuleCompletions
            .AnyAsync(mc => mc.EnrollmentId == enrollment.Id && mc.ModuleId == request.ModuleId, ct);
        if (!alreadyCompleted)
        {
            _context.ModuleCompletions.Add(new ModuleCompletion
            {
                EnrollmentId = enrollment.Id,
                ModuleId = request.ModuleId,
                CompletedAt = DateTime.UtcNow
            });
        }

        var totalModules = await _context.Modules.CountAsync(m => m.TrainingId == trainingId, ct);
        // The new completion is not persisted yet, so add it to the persisted count.
        var completedCount = await _context.ModuleCompletions.CountAsync(mc => mc.EnrollmentId == enrollment.Id, ct)
                             + (alreadyCompleted ? 0 : 1);
        enrollment.ProgressPercent = totalModules == 0 ? 0 : (int)Math.Round(completedCount * 100.0 / totalModules);
        if (totalModules > 0 && completedCount >= totalModules)
        {
            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.CompletedAt ??= DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        return Result<ProgressDto>.Success(await BuildProgressAsync(enrollment, ct));
    }

    public async Task<Result<QuizResultDto>> SubmitQuizAsync(Guid studentId, Guid trainingId, SubmitQuizRequest request, CancellationToken ct = default)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.TrainingId == trainingId, ct);
        if (enrollment is null)
            return Result<QuizResultDto>.NotFound("You are not enrolled in this training.");

        var assessment = await _context.Activities.OfType<Assessment>()
            .Include(a => a.Questions).ThenInclude(q => q.Answers)
            .Include(a => a.Module)
            .FirstOrDefaultAsync(a => a.Id == request.ActivityId, ct);
        if (assessment is null)
            return Result<QuizResultDto>.NotFound("Quiz or exam not found.");
        if (assessment.Module!.TrainingId != trainingId)
            return Result<QuizResultDto>.Validation("This quiz does not belong to the specified training.");

        var submitted = request.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedAnswerIds.ToHashSet());

        var totalPoints = assessment.Questions.Sum(q => q.Points);
        var earnedPoints = 0;
        var correctCount = 0;

        foreach (var question in assessment.Questions)
        {
            var correctIds = question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToHashSet();
            var chosen = submitted.TryGetValue(question.Id, out var set) ? set : new HashSet<Guid>();
            if (chosen.SetEquals(correctIds))
            {
                earnedPoints += question.Points;
                correctCount++;
            }
        }

        var score = totalPoints == 0 ? 0 : (int)Math.Round(earnedPoints * 100.0 / totalPoints);
        var passed = score >= assessment.PassingScore;

        _context.QuizAttempts.Add(new QuizAttempt
        {
            EnrollmentId = enrollment.Id,
            ActivityId = assessment.Id,
            Score = score,
            Passed = passed,
            SubmittedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(ct);

        return Result<QuizResultDto>.Success(new QuizResultDto(
            assessment.Id, score, passed, correctCount, assessment.Questions.Count, DateTime.UtcNow));
    }

    public async Task<Result<CertificateDto>> GetCertificateAsync(Guid studentId, Guid trainingId, CancellationToken ct = default)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Training)
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.TrainingId == trainingId, ct);
        if (enrollment is null)
            return Result<CertificateDto>.NotFound("You are not enrolled in this training.");

        var user = await _userManager.FindByIdAsync(studentId.ToString());
        var studentName = user?.FullName ?? "Student";
        var available = enrollment.Status == EnrollmentStatus.Completed;

        // Certificate generation is planned; expose readiness state for now.
        return Result<CertificateDto>.Success(new CertificateDto(
            enrollment.Id, trainingId, enrollment.Training!.Title, studentName, available,
            available ? enrollment.CompletedAt : null,
            available
                ? "Certificate is available. PDF generation is coming soon."
                : "Complete all modules to unlock your certificate."));
    }

    private async Task<ProgressDto> BuildProgressAsync(Enrollment enrollment, CancellationToken ct)
    {
        var modules = await _context.Modules.AsNoTracking()
            .Where(m => m.TrainingId == enrollment.TrainingId)
            .OrderBy(m => m.Order)
            .Select(m => new { m.Id, m.Title, m.Order })
            .ToListAsync(ct);

        // Query completions from the store (keyed by module) rather than a tracked navigation.
        var completedMap = await _context.ModuleCompletions.AsNoTracking()
            .Where(mc => mc.EnrollmentId == enrollment.Id)
            .GroupBy(mc => mc.ModuleId)
            .Select(g => new { ModuleId = g.Key, CompletedAt = g.Min(x => x.CompletedAt) })
            .ToDictionaryAsync(x => x.ModuleId, x => x.CompletedAt, ct);

        var moduleDtos = modules.Select(m => new ModuleProgressDto(
            m.Id, m.Title, m.Order,
            completedMap.ContainsKey(m.Id),
            completedMap.TryGetValue(m.Id, out var at) ? at : null)).ToList();

        return new ProgressDto(enrollment.Id, enrollment.TrainingId, enrollment.Training?.Title,
            enrollment.ProgressPercent, enrollment.Status, moduleDtos);
    }

    private static EnrollmentDto ToDto(Enrollment e, string? trainingTitle) =>
        new(e.Id, e.StudentId, e.TrainingId, trainingTitle, e.EnrolledAt, e.ProgressPercent, e.Status, e.CompletedAt);
}
