using Core.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Core.Service.Host.ServiceCollectionExtensions
{
    public static class StatelessServiceRegistrator
    {
        public static StatelessServiceRegistrationPipe RegisterStatelessServices(
            this IServiceCollection services)
        {
            return new StatelessServiceRegistrationPipe(services);
        }
        

        public class StatelessServiceRegistrationPipe
        {
            private readonly IServiceCollection _services;

            public StatelessServiceRegistrationPipe(IServiceCollection services)
            {
                _services = services;
            }

            public StatelessServiceRegistrationPipe AddHttpService<TService, TServiceInterface>()
                where TServiceInterface : class
                where TService : class, TServiceInterface, IInternalHttpService
            {
                _services.AddSingleton<TServiceInterface, TService>();

                return this;
            }

            public StatelessServiceRegistrationPipe AddBackgroundService<TService>()
                where TService : BackgroundService
            {
                _services.AddSingleton<TService>();
                _services.AddHostedService(provider => provider.GetService<TService>());

                return this;
            }
        }
    }
}
