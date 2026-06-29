using Company.NameProject.Domain.Common;
using Company.NameProject.Domain.Repositories;
using Company.NameProject.Persistence;
using Company.NameProject.Shared.Common;

using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace Company.NameProject.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación genérica del repositorio con soporte para cualquier tipo de identificador.
    /// </summary>
    public class GenericRepository<T, TId> : IGenericRepository<T, TId>
        where T : AggregateRoot<TId>
    {
        protected readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
            => await _context.Set<T>().FindAsync([id], cancellationToken);

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _context.Set<T>().ToListAsync(cancellationToken);

        public async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var totalCount = await _context.Set<T>().CountAsync(cancellationToken);
            var items = await _context.Set<T>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>(items, totalCount, page, pageSize);
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> expression)
            => _context.Set<T>().Where(expression);

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await _context.Set<T>().AddAsync(entity, cancellationToken);

        public void Update(T entity) => _context.Set<T>().Update(entity);

        public void Remove(T entity) => _context.Set<T>().Remove(entity);
    }

    /// <summary>
    /// Alias de <see cref="GenericRepository{T, TId}"/> con <c>Guid</c> como identificador.
    /// Mantiene compatibilidad con el código existente basado en Guid.
    /// </summary>
    public class GenericRepository<T> : GenericRepository<T, Guid>, IGenericRepository<T>
        where T : AggregateRoot
    {
        public GenericRepository(AppDbContext context) : base(context) { }
    }
}
