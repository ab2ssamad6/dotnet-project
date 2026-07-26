using System.Linq.Expressions;
using Lms.Domain.Common;

namespace Lms.Application.Abstractions;

/// <summary>
/// Thin generic repository over an aggregate root. Used by simple CRUD services;
/// services needing complex, join-heavy queries use the DbContext directly instead.
/// </summary>
public interface IRepository<T> where T : AuditableEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    IQueryable<T> Query();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
