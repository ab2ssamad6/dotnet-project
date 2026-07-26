using AutoMapper;
using Lms.Application.Abstractions;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Trainers;
using Lms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lms.Infrastructure.Services;

public class TrainerService : ITrainerService
{
    private readonly IRepository<Trainer> _repository;
    private readonly IMapper _mapper;

    public TrainerService(IRepository<Trainer> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<TrainerDto>>> GetPagedAsync(PagedQuery query, CancellationToken ct = default)
    {
        var q = _repository.Query().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(t => t.FirstName.Contains(query.Search)
                             || t.LastName.Contains(query.Search)
                             || t.Email.Contains(query.Search));

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(t => t.LastName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var dtos = _mapper.Map<List<TrainerDto>>(items);
        return Result<PagedResult<TrainerDto>>.Success(
            new PagedResult<TrainerDto>(dtos, total, query.Page, query.PageSize));
    }

    public async Task<Result<TrainerDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var trainer = await _repository.GetByIdAsync(id, ct);
        return trainer is null
            ? Result<TrainerDto>.NotFound("Trainer not found.")
            : Result<TrainerDto>.Success(_mapper.Map<TrainerDto>(trainer));
    }

    public async Task<Result<TrainerDto>> CreateAsync(CreateTrainerRequest request, CancellationToken ct = default)
    {
        if (await _repository.AnyAsync(t => t.Email == request.Email, ct))
            return Result<TrainerDto>.Conflict("A trainer with this email already exists.");

        var trainer = new Trainer
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Biography = request.Biography,
            Avatar = request.Avatar,
            Expertise = request.Expertise,
            Phone = request.Phone
        };
        await _repository.AddAsync(trainer, ct);
        await _repository.SaveChangesAsync(ct);
        return Result<TrainerDto>.Success(_mapper.Map<TrainerDto>(trainer));
    }

    public async Task<Result<TrainerDto>> UpdateAsync(Guid id, UpdateTrainerRequest request, CancellationToken ct = default)
    {
        var trainer = await _repository.GetByIdAsync(id, ct);
        if (trainer is null)
            return Result<TrainerDto>.NotFound("Trainer not found.");

        if (await _repository.AnyAsync(t => t.Email == request.Email && t.Id != id, ct))
            return Result<TrainerDto>.Conflict("A trainer with this email already exists.");

        trainer.FirstName = request.FirstName;
        trainer.LastName = request.LastName;
        trainer.Email = request.Email;
        trainer.Biography = request.Biography;
        trainer.Avatar = request.Avatar;
        trainer.Expertise = request.Expertise;
        trainer.Phone = request.Phone;
        _repository.Update(trainer);
        await _repository.SaveChangesAsync(ct);
        return Result<TrainerDto>.Success(_mapper.Map<TrainerDto>(trainer));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var trainer = await _repository.Query()
            .Include(t => t.Trainings)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        if (trainer is null)
            return Result.NotFound("Trainer not found.");
        if (trainer.Trainings.Any())
            return Result.Conflict("Cannot delete a trainer who still has trainings.");

        _repository.Remove(trainer);
        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
