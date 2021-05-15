using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.Tool;

namespace Core.Service.Host.Client.DynamicProxy.Http
{
    internal interface IHttpServiceDynamicInstance
    {
        Task CallAsync(string path, HttpContent content, CancellationToken token);
        Task<T> CallAsync<T>(string path, HttpContent content, CancellationToken token);
    }

    //TODO: handle non success responses
    public class HttpServiceDynamicInstance : IHttpServiceDynamicInstance
    {
        private readonly HttpClient _client;

        public HttpServiceDynamicInstance(HttpClient client)
        {
            _client = client;
        }

        public async Task CallAsync(string path, HttpContent content, CancellationToken token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = content
            };
            
            await _client.SendAsync(request, token);
        }

        public async Task<T> CallAsync<T>(string path, HttpContent content, CancellationToken token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = content
            };

            var result = await _client.SendAsync(request, token);

            return Tools.Json.Serializer.Deserialize<T>(await result.Content.ReadAsStringAsync());
        }
    }
}