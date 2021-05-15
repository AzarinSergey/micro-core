using Core.Service.Host.Convention.Convention;
using Core.Tool;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Service.Host.ApplicationBuilderExtensions
{
    public static class ServiceEndpointMapper
    {
        public static IApplicationBuilder UseServiceEndpoints(this IApplicationBuilder app, Type[] serviceTypes, IServiceEndpointConvention serviceEndpointConvention)
        {
            foreach (var serviceType in serviceTypes)
            {
                var instance = app.ApplicationServices.GetService(serviceType);
                var methods = serviceType.GetMethods();

                app.UseEndpoints(builder =>
                {
                    foreach (var methodInfo in methods)
                    {
                        ThrowExceptionIfMethodCanNotBeMapped(methodInfo);
                        var path = serviceEndpointConvention.GetServiceMethodPath(methodInfo, serviceType);
                        builder.MapPost(path, context => MapPost(context, methodInfo, instance));
                    }
                });
            }
            return app;
        }

        public static IApplicationBuilder UseServiceEndpoints(this IApplicationBuilder app, Type[] serviceTypes)
            =>    UseServiceEndpoints(app, serviceTypes,
                app.ApplicationServices.GetRequiredService<IServiceEndpointConvention>());

        private static async Task MapPost(HttpContext context, MethodInfo methodInfo, object instance)
        {
            string jsonBody;

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                jsonBody = await reader.ReadToEndAsync();

            var methodParams = methodInfo.GetParameters();
            var requestParams = new object[methodParams.Length];

            if (!string.IsNullOrEmpty(jsonBody))
            {
                var requestParamsJson = JsonDocument.Parse(jsonBody);

                for (var i = 0; i < methodParams.Length; i++)
                {
                    if (methodParams[i].ParameterType.FullName == typeof(CancellationToken).FullName)
                        continue;

                    if (requestParamsJson.RootElement.TryGetProperty(methodParams[i].Name, out var element))
                    {
                        requestParams[i] = Tools.Json.Serializer.Deserialize(element.GetRawText(), methodParams[i].ParameterType);
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Request parameter not found by name: '{methodParams[i].Name}' ");
                    }
                }
            }

            requestParams[^1] = context.RequestAborted;

            var task = (Task)methodInfo.Invoke(instance, requestParams);

            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            var resultValue = resultProperty?.GetValue(task);

            await context.Response.WriteAsync(Tools.Json.Serializer.Serialize(resultValue), context.RequestAborted);
        }

        private static void ThrowExceptionIfMethodCanNotBeMapped(MethodInfo methodInfo)
        {
            //TODO: ThrowExceptionIfMethodCanNotBeMapped
        }
    }
}