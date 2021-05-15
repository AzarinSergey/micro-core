using System;
using System.Threading.Tasks;

namespace Moedi.Data.Core.Access
{
    public interface IUow : IDisposable,
        ICommandRepositoryFactory,
        IQueryRepositoryFactory
    {
        Task Commit();
        Task Rollback();
    }
}