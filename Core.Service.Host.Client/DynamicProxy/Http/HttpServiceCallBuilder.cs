using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Core.Service.Host.Client.DynamicProxy.Http.Content;
using Core.Service.Host.Convention.Convention;

namespace Core.Service.Host.Client.DynamicProxy.Http
{
    internal interface IHttpServiceCallBuilder
    {
        IHttpServiceCallBuilder BuildFor(IInvocation invocation);
        Task CallAsync();
        object CallAsyncWithResult();
    }

    internal class HttpServiceCallBuilder : IHttpServiceCallBuilder
    {
        private readonly IHttpServiceDynamicInstance _dynamicProxy;
        private readonly Type _serviceType;
        private readonly IServiceEndpointConvention _serviceEndpointConvention;
        private IInvocation _invocation;

        private HttpContent _requestContent;
        private CancellationToken _cancellationToken;

        public HttpServiceCallBuilder(IHttpServiceDynamicInstance dynamicProxy,
            Type serviceType, IServiceEndpointConvention serviceEndpointConvention)
        {
            _dynamicProxy = dynamicProxy;
            _serviceType = serviceType;
            _serviceEndpointConvention = serviceEndpointConvention;
        }

        public IHttpServiceCallBuilder BuildFor(IInvocation invocation)
        {
            //invocation
            _invocation = invocation;

            //token
            var tokenObj = invocation.Arguments.FirstOrDefault(x => x is CancellationToken);
            if (tokenObj == null) _cancellationToken = CancellationToken.None;
            else _cancellationToken = (CancellationToken) tokenObj;

            //content
            var requestBuilder = new DefaultRequestContentBuilder(_serviceType, invocation.Method);
            _requestContent = requestBuilder.BuildContent(_invocation.Arguments);

            //next
            //TODO: get request meta (headers, method type, content type) from service interface type attributes

            return this;
        }

        #region Call

        public async Task CallAsync()
        {
            using (_requestContent)
                await _dynamicProxy.CallAsync(
                    _serviceEndpointConvention.GetServiceMethodPath(_invocation.Method, _serviceType), _requestContent, _cancellationToken);
        }

        public object CallAsyncWithResult()
        {
            var resultType = _invocation.Method.ReturnType.GetGenericArguments()[0];
            var mi = ProcessAsyncWithResultMethodInfo.MakeGenericMethod(resultType);

            return mi.Invoke(this, null);
        }

        private static readonly MethodInfo HandleAsyncMethodInfo = typeof(HttpServiceCallBuilder)
            .GetMethod("BuildAsyncWithResult", BindingFlags.Instance | BindingFlags.NonPublic);
        private MethodInfo ProcessAsyncWithResultMethodInfo => HandleAsyncMethodInfo;
        private async Task<T> BuildAsyncWithResult<T>()
        {
            using (_requestContent)
                return await _dynamicProxy.CallAsync<T>(_serviceEndpointConvention.GetServiceMethodPath(_invocation.Method, _serviceType), _requestContent, _cancellationToken);
        }

        #endregion
    }
}