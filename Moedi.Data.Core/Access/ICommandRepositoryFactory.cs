using Moedi.Data.Core.Entity;

namespace Moedi.Data.Core.Access
{
    public interface ICommandRepositoryFactory
    {
        ICommandRepository<TEntity> GetRepository<TEntity>()
            where TEntity : class, IId;
    }
}