using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CentralizedSalesSystem.Frontend.Services
{
    public class LoggingMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Log request
            Console.WriteLine($"==> {request.Method} {request.RequestUri}");
            
            // Log auth header status
            if (request.Headers.Authorization != null)
            {
                Console.WriteLine($"Auth: {request.Headers.Authorization.Scheme} [token present]");
            }
            else
            {
                Console.WriteLine("Auth: No token");
            }
            
            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrEmpty(requestBody))
                {
                    Console.WriteLine($"Request Body: {requestBody}");
                }
            }

            // Send request
            var response = await base.SendAsync(request, cancellationToken);

            // Log response
            Console.WriteLine($"<== {(int)response.StatusCode} {response.ReasonPhrase}");
            
            if (response.Content != null)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrEmpty(responseBody) && responseBody.Length < 5000)
                {
                    Console.WriteLine($"Response Body: {responseBody}");
                }
                else if (!string.IsNullOrEmpty(responseBody))
                {
                    Console.WriteLine($"Response Body: [Too large: {responseBody.Length} chars]");
                }
            }

            Console.WriteLine("---");
            return response;
        }
    }
}
