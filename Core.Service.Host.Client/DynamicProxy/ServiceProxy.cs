using System;
using System.Net.Http;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Core.Service.Host.Client.DynamicProxy.Http;
using Core.Service.Host.Convention.Convention;
using Core.Service.Interfaces;

namespace Core.Service.Host.Client.DynamicProxy
{
    public class ServiceProxy<T>
        where T : IInternalHttpService
    {
        private readonly IServiceEndpointConvention _endpointConvention;
        private readonly HttpClient _client;

        public ServiceProxy(IServiceEndpointConvention endpointConvention, HttpClient client)
        {
            _endpointConvention = endpointConvention;
            _client = client;
        }

        public T Call()
        {
            var serviceInterfaceType = typeof(T);
            if (!serviceInterfaceType.IsInterface)
                throw new ArgumentException($"'TService' - interface type only allowed.");

            var httpServiceDynamicInstance = new HttpServiceDynamicInstance(_client);

            var httpCallBuilder = new HttpServiceCallBuilder(
                httpServiceDynamicInstance,
                serviceInterfaceType,
                _endpointConvention);

            return (T)new ProxyGenerator().CreateInterfaceProxyWithoutTarget(
                typeof(T), new HttpInterfaceInterceptor(httpCallBuilder));
        }

        private class HttpInterfaceInterceptor : IInterceptor
        {
            private readonly IHttpServiceCallBuilder _builder;

            public HttpInterfaceInterceptor(IHttpServiceCallBuilder builder)
            {
                _builder = builder;
            }

            public void Intercept(IInvocation invocation)
            {
                var returnType = invocation.Method.ReturnType;

                _builder.BuildFor(invocation);

                if (returnType == typeof(Task))
                {
                    invocation.ReturnValue = _builder.CallAsync();
                    return;
                }

                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    invocation.ReturnValue = _builder.CallAsyncWithResult();
                    return;
                }

                throw new NotSupportedException($"Return type '{returnType}' of method '{invocation.Method.Name}' not supported. The types 'Task' and 'Task <>' are only supported.");
            }
        }

    }
}