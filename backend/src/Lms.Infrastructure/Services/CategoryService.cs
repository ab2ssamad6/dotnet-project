using AutoMapper;
using Lms.Application.Abstractions;
using Lms.Application.Abstractions.Services;
using Lms.Application.Common;
using Lms.Application.Dtos.Categories;
using Lms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lms.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _repository;
    private readonly IMapper _mapper;

    public CategoryService(IRepository<Category> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CategoryDto>>> GetPagedAsync(PagedQuery query, CancellationToken ct = default)
    {
        var q = _repository.Query().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(c => c.Name.Contains(query.Search));

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(c => c.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var dtos = _mapper.Map<List<CategoryDto>>(items);
        return Result<PagedResult<CategoryDto>>.Success(
            new PagedResult<CategoryDto>(dtos, total, query.Page, query.PageSize));
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        return category is null
            ? Result<CategoryDto>.NotFound("Category not found.")
            : Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        if (await _repository.AnyAsync(c => c.Name == request.Name, ct))
            return Result<CategoryDto>.Conflict("A category with this name already exists.");

        var category = new Category { Name = request.Name, Description = request.Description };
        await _repository.AddAsync(category, ct);
        await _repository.SaveChangesAsync(ct);
        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
    }

    public async Task<Result<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _repository.GetByIdAsync(id, ct);
        if (category is null)
            return Result<CategoryDto>.NotFound("Category not found.");

        if (await _repository.AnyAsync(c => c.Name == request.Name && c.Id != id, ct))
            return Result<CategoryDto>.Conflict("A category with this name already exists.");

        category.Name = request.Name;
        category.Description = request.Description;
        _repository.Update(category);
        await _repository.SaveChangesAsync(ct);
        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _repository.Query()
            .Include(c => c.Trainings)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
            return Result.NotFound("Category not found.");
        if (category.Trainings.Any())
            return Result.Conflict("Cannot delete a category that still has trainings.");

        _repository.Remove(category);
        await _repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
