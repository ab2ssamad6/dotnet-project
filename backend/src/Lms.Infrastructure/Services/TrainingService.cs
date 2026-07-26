using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Trainings;
using Lms.Domain.Entities;
using Lms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lms.Infrastructure.Services;

public class TrainingService : ITrainingService
{
    private readonly LmsDbContext _context;

    public TrainingService(LmsDbContext context) => _context = context;

    public async Task<Result<PagedResult<TrainingDto>>> GetPagedAsync(PagedQuery query, bool onlyPublished, CancellationToken ct = default)
    {
        var q = _context.Trainings.AsNoTracking();
        if (onlyPublished)
            q = q.Where(t => t.Published);
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(t => t.Title.Contains(query.Search));

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(t => t.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(Projection)
            .ToListAsync(ct);

        return Result<PagedResult<TrainingDto>>.Success(
            new PagedResult<TrainingDto>(items, total, query.Page, query.PageSize));
    }

    public async Task<Result<TrainingDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dto = await _context.Trainings.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(Projection)
            .FirstOrDefaultAsync(ct);
        return dto is null
            ? Result<TrainingDto>.NotFound("Training not found.")
            : Result<TrainingDto>.Success(dto);
    }

    public async Task<Result<TrainingDto>> CreateAsync(CreateTrainingRequest request, CancellationToken ct = default)
    {
        if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, ct))
            return Result<TrainingDto>.Validation("The specified category does not exist.");
        if (!await _context.Trainers.AnyAsync(t => t.Id == request.TrainerId, ct))
            return Result<TrainingDto>.Validation("The specified trainer does not exist.");

        var training = new Training
        {
            Title = request.Title,
            Description = request.Description,
            Difficulty = request.Difficulty,
            Duration = request.Duration,
            Thumbnail = request.Thumbnail,
            Status = request.Status,
            Published = request.Published,
            CategoryId = request.CategoryId,
            TrainerId = request.TrainerId
        };
        _context.Trainings.Add(training);
        await _context.SaveChangesAsync(ct);
        return await GetByIdAsync(training.Id, ct);
    }

    public async Task<Result<TrainingDto>> UpdateAsync(Guid id, UpdateTrainingRequest request, CancellationToken ct = default)
    {
        var training = await _context.Trainings.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (training is null)
            return Result<TrainingDto>.NotFound("Training not found.");
        if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, ct))
            return Result<TrainingDto>.Validation("The specified category does not exist.");
        if (!await _context.Trainers.AnyAsync(t => t.Id == request.TrainerId, ct))
            return Result<TrainingDto>.Validation("The specified trainer does not exist.");

        training.Title = request.Title;
        training.Description = request.Description;
        training.Difficulty = request.Difficulty;
        training.Duration = request.Duration;
        training.Thumbnail = request.Thumbnail;
        training.Status = request.Status;
        training.Published = request.Published;
        training.CategoryId = request.CategoryId;
        training.TrainerId = request.TrainerId;
        await _context.SaveChangesAsync(ct);
        return await GetByIdAsync(training.Id, ct);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var training = await _context.Trainings.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (training is null)
            return Result.NotFound("Training not found.");

        _context.Trainings.Remove(training);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    // Reusable entity → DTO projection evaluated in SQL.
    private static readonly System.Linq.Expressions.Expression<Func<Training, TrainingDto>> Projection = t =>
        new TrainingDto(
            t.Id,
            t.Title,
            t.Description,
            t.Difficulty,
            t.Duration,
            t.Thumbnail,
            t.Status,
            t.Published,
            t.CategoryId,
            t.Category!.Name,
            t.TrainerId,
            t.Trainer!.FirstName + " " + t.Trainer.LastName,
            t.Modules.Count(),
            t.CreatedAt,
            t.UpdatedAt);
}
