using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Moedi.Data.Core.Access;

namespace Moedi.Data.Ef
{
    public class UowFactory<TDbContext> : IUowFactory
        where TDbContext : MoediDbContext
    {
        private readonly IDbContextFactory<TDbContext> _factory;

        public UowFactory(IDbContextFactory<TDbContext> factory)
        {
            _factory = factory;
        }

        IUow IUowFactory.CreateUnitOfWork(Guid? transactionUuid, CancellationToken token)
        {
            var dbContext = _factory.CreateDbContext();

            return new UnitOfWork<TDbContext>(dbContext, transactionUuid.HasValue, token);
        }
    }
}