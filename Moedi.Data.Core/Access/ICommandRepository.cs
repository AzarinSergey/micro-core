using Moedi.Data.Core.Entity;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Moedi.Data.Core.Access
{
    public interface ICommandRepository<TEntity> : IQueryRepository<TEntity>
         where TEntity : class, IId
    {
        Task<int> CreateOrUpdateAsync(TEntity entity);

        Task<int> Update(Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TEntity>> updateExpression);

        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> condition);
    }
}
