using Core.Service.Interfaces;
using System;

namespace Moedi.Cqrs
{
    public class ExternalServiceProvider : IExternalServiceProvider
    {
        private readonly IServiceProvider _provider;

        public ExternalServiceProvider(IServiceProvider provider)
        {
            _provider = provider;
        }

        public T GetExternalHttpService<T>()
            where T : IExternalHttpService
        {
            return (T)_provider.GetService(typeof(T));
        }
    }
}