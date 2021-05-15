using System;
using System.Threading;

namespace Moedi.Data.Core.Access
{
    public interface IUowFactory
    {
        IUow CreateUnitOfWork(Guid? transactionUuid, CancellationToken token = default);
    }
}