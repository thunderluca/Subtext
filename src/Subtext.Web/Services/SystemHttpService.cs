using log4net;
using Subtext.Azure.Search.Services;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Subtext.Web.Services
{
    public class SystemHttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILog _logger;

        public SystemHttpService(string apiKey, ILog logger = null)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException($"{nameof(apiKey)} cannot be null, empty or blank", nameof(apiKey));
            }

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("api-key", apiKey);
            _logger = logger;
        }

        public async Task<string> PostContentAsync(string content, string mimeType, string url)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException($"{nameof(content)} cannot be null, empty or blank", nameof(content));
            }

            if (string.IsNullOrWhiteSpace(mimeType))
            {
                throw new ArgumentException($"{nameof(mimeType)} cannot be null, empty or blank", nameof(mimeType));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException($"{nameof(url)} cannot be null, empty or blank", nameof(url));
            }

            string responseContent = null;

            try
            {
                var response = await _httpClient.PostAsync(url, new StringContent(content, Encoding.UTF8, mimeType)).ConfigureAwait(false);

                responseContent = response.Content != null 
                    ? await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                    : null;

                response.EnsureSuccessStatusCode();

                return responseContent;
            }
            catch (Exception ex)
            {
                _logger?.Error($"An error occured while request data: {ex.Message}, response body: {responseContent ?? "(no response body)"}");

                return null;
            }
        }
    }
}
