using System.Linq.Expressions;
using Lms.Application.Abstractions;
using Lms.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Lms.Infrastructure.Persistence;

/// <summary>Generic EF Core implementation of <see cref="IRepository{T}"/>.</summary>
public class EfRepository<T> : IRepository<T> where T : AuditableEntity
{
    private readonly LmsDbContext _context;
    private readonly DbSet<T> _set;

    public EfRepository(LmsDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _set.AnyAsync(e => e.Id == id, cancellationToken);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        _set.AnyAsync(predicate, cancellationToken);

    public Task<List<T>> ListAsync(CancellationToken cancellationToken = default) =>
        _set.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public IQueryable<T> Query() => _set.AsQueryable();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
