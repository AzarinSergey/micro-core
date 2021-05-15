using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Core.Tool;

namespace Core.Service.Host.Client.DynamicProxy.Http.Content
{
    internal class DefaultRequestContentBuilder
    {
        private readonly Type _serviceType;
        private readonly MethodInfo _methodInfo;

        public DefaultRequestContentBuilder(Type serviceType, MethodInfo methodInfo)
        {
            _serviceType = serviceType;
            _methodInfo = methodInfo;
        }

        public StringContent BuildContent(object[] arguments)
        {
            var parameters = _methodInfo.GetParameters();

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("{");

            for (int i = 0; i < arguments.Length; i++)
            {
                var propertyName = parameters[i].Name;
                var propertyValue = Tools.Json.Serializer.Serialize(arguments[i]);

                stringBuilder.Append($"\"{propertyName}\":{propertyValue}");
                stringBuilder.Append(",");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append("}");

            return new StringContent(stringBuilder.ToString());
        }
    }
}