using Company.NameProject.Domain.Common;
using Company.NameProject.Shared.Common;

using System.Linq.Expressions;

namespace Company.NameProject.Domain.Repositories
{
    /// <summary>
    /// Repositorio genérico que soporta cualquier tipo de identificador.
    /// </summary>
    public interface IGenericRepository<T, TId> where T : AggregateRoot<TId>
    {
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        IQueryable<T> Find(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Remove(T entity);
    }

    /// <summary>
    /// Alias de <see cref="IGenericRepository{T, TId}"/> con <c>Guid</c> como identificador.
    /// Mantiene compatibilidad con el código existente basado en Guid.
    /// </summary>
    public interface IGenericRepository<T> : IGenericRepository<T, Guid> where T : AggregateRoot { }
}
