using System.Linq;
using Moedi.Data.Core.Entity;

namespace Moedi.Data.Core.Access
{
    public interface IQueryRepository<TEntity>
        where TEntity : class, IId
    {
        IQueryable<TEntity> Query();
    }
}