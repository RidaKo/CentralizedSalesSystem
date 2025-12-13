using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace CentralizedSalesSystem.Frontend.Json
{
    public static class HttpJsonDefaultsExtensions
    {
        public static Task<HttpResponseMessage> PostAsJsonAsync<TValue>(
            this HttpClient client,
            string? requestUri,
            TValue value,
            CancellationToken cancellationToken = default) =>
            HttpClientJsonExtensions.PostAsJsonAsync(client, requestUri, value, JsonDefaults.Options, cancellationToken);

        public static Task<HttpResponseMessage> PutAsJsonAsync<TValue>(
            this HttpClient client,
            string? requestUri,
            TValue value,
            CancellationToken cancellationToken = default) =>
            HttpClientJsonExtensions.PutAsJsonAsync(client, requestUri, value, JsonDefaults.Options, cancellationToken);

        public static Task<HttpResponseMessage> PatchAsJsonAsync<TValue>(
            this HttpClient client,
            string? requestUri,
            TValue value,
            CancellationToken cancellationToken = default) =>
            HttpClientJsonExtensions.PatchAsJsonAsync(client, requestUri, value, JsonDefaults.Options, cancellationToken);

        public static Task<TValue?> GetFromJsonAsync<TValue>(
            this HttpClient client,
            string? requestUri,
            CancellationToken cancellationToken = default) =>
            HttpClientJsonExtensions.GetFromJsonAsync<TValue>(client, requestUri, JsonDefaults.Options, cancellationToken);

        public static Task<TValue?> ReadFromJsonAsync<TValue>(
            this HttpContent content,
            CancellationToken cancellationToken = default) =>
            HttpContentJsonExtensions.ReadFromJsonAsync<TValue>(content, JsonDefaults.Options, cancellationToken);
    }
}
