using System;
using System.Reflection;

namespace Core.Service.Host.Convention.Convention
{
    public interface IServiceEndpointConvention
    {
        string GetServiceMethodPath(MethodInfo method, Type serviceInterfaceType);
    }
}