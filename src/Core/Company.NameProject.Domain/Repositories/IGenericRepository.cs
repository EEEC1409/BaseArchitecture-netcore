using Company.NameProject.Domain.Common;
using Company.NameProject.Shared.Common;

using System.Linq.Expressions;

namespace Company.NameProject.Domain.Repositories
{
    public interface IGenericRepository<T> where T : AggregateRoot
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        IQueryable<T> Find(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Remove(T entity);
    }
}

