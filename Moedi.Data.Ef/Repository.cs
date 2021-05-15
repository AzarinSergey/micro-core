using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moedi.Data.Core.Access;
using Moedi.Data.Core.Entity;
using Z.EntityFramework.Plus;

namespace Moedi.Data.Ef
{
    public sealed class Repository<TEntity> : ICommandRepository<TEntity>
        where TEntity : class, IId
    {
        private readonly MoediDbContext _context;
        private readonly CancellationToken _token;

        public Repository(MoediDbContext context, CancellationToken token)
        {
            _context = context;
            _token = token;
        }

        public async Task<int> CreateOrUpdateAsync(TEntity entity)
        {
            if (entity.Id == default)
            {
                await _context.Set<TEntity>().AddAsync(entity, _token);
            }
            await _context.SaveChangesAsync(_token);

            return entity.Id;
        }

        public async Task<int> Update(Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TEntity>> updateExpression)
        {
            var result = await _context.Set<TEntity>().Where(condition).UpdateAsync(updateExpression, _token);
            await _context.SaveChangesAsync(_token);

            return result;
        }

        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> condition)
        {
            var removedCount = await _context.Set<TEntity>().Where(condition).DeleteAsync(_token);
            await _context.SaveChangesAsync(_token);

            return removedCount;
        }

        public IQueryable<TEntity> Query()
            => _context.Set<TEntity>();
    }
}
