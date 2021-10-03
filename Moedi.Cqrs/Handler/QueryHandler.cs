using Microsoft.Extensions.Logging;
using Moedi.Data.Core.Access;
using System.Threading;
using System.Threading.Tasks;

namespace Moedi.Cqrs.Handler
{
    public abstract class QueryHandler<TResult>
    {
        internal ILogger Logger;

        protected ILogger UseLogger => Logger;

        public abstract Task<TResult> Query(IQueryRepositoryFactory f, CancellationToken token);
    }
}