using Core.Service.Interfaces;

namespace Moedi.Cqrs
{
    public interface IExternalServiceProvider
    {
        T GetExternalHttpService<T>()
            where T : IExternalHttpService;
    }
}