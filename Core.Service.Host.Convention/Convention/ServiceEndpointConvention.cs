using System;
using System.Reflection;

namespace Core.Service.Host.Convention.Convention
{
    public class ServiceEndpointConvention : IServiceEndpointConvention
    {
        public string GetServiceMethodPath(MethodInfo methodInfo, Type serviceInterfaceType)
        {
            var serviceInterfaceName = serviceInterfaceType.Name;
            return $"{(serviceInterfaceName.StartsWith("I") ? serviceInterfaceName.Substring(1) : serviceInterfaceName)}/{methodInfo.Name}";
        }
    }
}