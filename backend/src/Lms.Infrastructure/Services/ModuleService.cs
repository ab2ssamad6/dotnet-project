using AutoMapper;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Modules;
using Lms.Domain.Entities;
using Lms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lms.Infrastructure.Services;

public class ModuleService : IModuleService
{
    private readonly LmsDbContext _context;
    private readonly IMapper _mapper;

    public ModuleService(LmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<ModuleDto>>> GetByTrainingAsync(Guid trainingId, CancellationToken ct = default)
    {
        if (!await _context.Trainings.AnyAsync(t => t.Id == trainingId, ct))
            return Result<IReadOnlyList<ModuleDto>>.NotFound("Training not found.");

        var modules = await _context.Modules.AsNoTracking()
            .Where(m => m.TrainingId == trainingId)
            .OrderBy(m => m.Order)
            .ToListAsync(ct);

        return Result<IReadOnlyList<ModuleDto>>.Success(_mapper.Map<List<ModuleDto>>(modules));
    }

    public async Task<Result<ModuleDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var module = await _context.Modules.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
        return module is null
            ? Result<ModuleDto>.NotFound("Module not found.")
            : Result<ModuleDto>.Success(_mapper.Map<ModuleDto>(module));
    }

    public async Task<Result<ModuleDto>> CreateAsync(CreateModuleRequest request, CancellationToken ct = default)
    {
        if (!await _context.Trainings.AnyAsync(t => t.Id == request.TrainingId, ct))
            return Result<ModuleDto>.Validation("The specified training does not exist.");

        var module = new Module
        {
            TrainingId = request.TrainingId,
            Title = request.Title,
            Description = request.Description,
            Order = request.Order,
            Duration = request.Duration,
            VideoUrl = request.VideoUrl,
            Attachment = request.Attachment,
            AiAvatarEnabled = request.AiAvatarEnabled
        };
        _context.Modules.Add(module);
        await _context.SaveChangesAsync(ct);
        return Result<ModuleDto>.Success(_mapper.Map<ModuleDto>(module));
    }

    public async Task<Result<ModuleDto>> UpdateAsync(Guid id, UpdateModuleRequest request, CancellationToken ct = default)
    {
        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (module is null)
            return Result<ModuleDto>.NotFound("Module not found.");

        module.Title = request.Title;
        module.Description = request.Description;
        module.Order = request.Order;
        module.Duration = request.Duration;
        module.VideoUrl = request.VideoUrl;
        module.Attachment = request.Attachment;
        module.AiAvatarEnabled = request.AiAvatarEnabled;
        await _context.SaveChangesAsync(ct);
        return Result<ModuleDto>.Success(_mapper.Map<ModuleDto>(module));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (module is null)
            return Result.NotFound("Module not found.");

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
