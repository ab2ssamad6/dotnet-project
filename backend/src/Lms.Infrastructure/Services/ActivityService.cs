using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Activities;
using Lms.Domain.Entities;
using Lms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lms.Infrastructure.Services;

public class ActivityService : IActivityService
{
    private readonly LmsDbContext _context;

    public ActivityService(LmsDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<ActivityDto>>> GetByModuleAsync(Guid moduleId, bool includeCorrectAnswers, CancellationToken ct = default)
    {
        if (!await _context.Modules.AnyAsync(m => m.Id == moduleId, ct))
            return Result<IReadOnlyList<ActivityDto>>.NotFound("Module not found.");

        var activities = await LoadWithQuestions(a => a.ModuleId == moduleId, ct);
        var dtos = activities.OrderBy(a => a.Order).Select(a => Map(a, includeCorrectAnswers)).ToList();
        return Result<IReadOnlyList<ActivityDto>>.Success(dtos);
    }

    public async Task<Result<ActivityDto>> GetByIdAsync(Guid id, bool includeCorrectAnswers, CancellationToken ct = default)
    {
        var activity = (await LoadWithQuestions(a => a.Id == id, ct)).FirstOrDefault();
        return activity is null
            ? Result<ActivityDto>.NotFound("Activity not found.")
            : Result<ActivityDto>.Success(Map(activity, includeCorrectAnswers));
    }

    public async Task<Result<ActivityDto>> CreateLessonAsync(Guid moduleId, CreateLessonRequest request, CancellationToken ct = default)
    {
        if (!await _context.Modules.AnyAsync(m => m.Id == moduleId, ct))
            return Result<ActivityDto>.Validation("The specified module does not exist.");

        var lesson = new Lesson
        {
            ModuleId = moduleId,
            Title = request.Title,
            Order = request.Order,
            Content = request.Content,
            VideoUrl = request.VideoUrl
        };
        _context.Activities.Add(lesson);
        await _context.SaveChangesAsync(ct);
        return Result<ActivityDto>.Success(Map(lesson, includeCorrectAnswers: true));
    }

    public async Task<Result<ActivityDto>> CreateExerciseAsync(Guid moduleId, CreateExerciseRequest request, CancellationToken ct = default)
    {
        if (!await _context.Modules.AnyAsync(m => m.Id == moduleId, ct))
            return Result<ActivityDto>.Validation("The specified module does not exist.");

        var exercise = new Exercise
        {
            ModuleId = moduleId,
            Title = request.Title,
            Order = request.Order,
            Instructions = request.Instructions,
            ExpectedOutcome = request.ExpectedOutcome
        };
        _context.Activities.Add(exercise);
        await _context.SaveChangesAsync(ct);
        return Result<ActivityDto>.Success(Map(exercise, includeCorrectAnswers: true));
    }

    public Task<Result<ActivityDto>> CreateQuizAsync(Guid moduleId, CreateQuizRequest request, CancellationToken ct = default) =>
        CreateAssessmentAsync(moduleId, new Quiz(), request.Title, request.Order, request.PassingScore,
            request.DurationMinutes, request.Questions, ct);

    public Task<Result<ActivityDto>> CreateExamAsync(Guid moduleId, CreateExamRequest request, CancellationToken ct = default) =>
        CreateAssessmentAsync(moduleId, new Exam(), request.Title, request.Order, request.PassingScore,
            request.DurationMinutes, request.Questions, ct);

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (activity is null)
            return Result.NotFound("Activity not found.");

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result<ActivityDto>> CreateAssessmentAsync(
        Guid moduleId, Assessment assessment, string title, int order, int passingScore,
        int? durationMinutes, IReadOnlyList<CreateQuestionRequest> questions, CancellationToken ct)
    {
        if (!await _context.Modules.AnyAsync(m => m.Id == moduleId, ct))
            return Result<ActivityDto>.Validation("The specified module does not exist.");

        assessment.ModuleId = moduleId;
        assessment.Title = title;
        assessment.Order = order;
        assessment.PassingScore = passingScore;
        assessment.DurationMinutes = durationMinutes;
        assessment.Questions = questions.Select(q => new Question
        {
            Text = q.Text,
            Type = q.Type,
            Points = q.Points,
            Answers = q.Answers.Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect }).ToList()
        }).ToList();

        _context.Activities.Add(assessment);
        await _context.SaveChangesAsync(ct);
        return Result<ActivityDto>.Success(Map(assessment, includeCorrectAnswers: true));
    }

    /// <summary>Loads matching activities and, for assessments, their questions and answers.</summary>
    private async Task<List<LearningActivity>> LoadWithQuestions(
        System.Linq.Expressions.Expression<Func<LearningActivity, bool>> predicate, CancellationToken ct)
    {
        var activities = await _context.Activities.Where(predicate).ToListAsync(ct);

        var assessmentIds = activities.OfType<Assessment>().Select(a => a.Id).ToList();
        if (assessmentIds.Count > 0)
        {
            // Relationship fixup attaches these to the tracked assessments above.
            await _context.Questions
                .Where(q => assessmentIds.Contains(q.AssessmentId))
                .Include(q => q.Answers)
                .LoadAsync(ct);
        }
        return activities;
    }

    private static ActivityDto Map(LearningActivity activity, bool includeCorrectAnswers) => activity switch
    {
        Lesson l => Base(l) with { Content = l.Content, VideoUrl = l.VideoUrl },
        Exercise e => Base(e) with { Instructions = e.Instructions, ExpectedOutcome = e.ExpectedOutcome },
        Assessment a => Base(a) with
        {
            PassingScore = a.PassingScore,
            DurationMinutes = a.DurationMinutes,
            Questions = a.Questions
                .Select(q => new QuestionDto(
                    q.Id, q.Text, q.Type, q.Points,
                    q.Answers.Select(ans => new AnswerDto(
                        ans.Id, ans.Text, includeCorrectAnswers ? ans.IsCorrect : null)).ToList()))
                .ToList()
        },
        _ => Base(activity)
    };

    private static ActivityDto Base(LearningActivity a) =>
        new(a.Id, a.ModuleId, a.ActivityType, a.Title, a.Order,
            null, null, null, null, null, null, null);
}
