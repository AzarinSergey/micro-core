using Moedi.Data.Core.Entity;

namespace Moedi.Data.Core.Access
{
    public interface IQueryRepositoryFactory
    {
        IQueryRepository<TEntity> GetRepository<TEntity>()
            where TEntity : class, IId;
    }
}