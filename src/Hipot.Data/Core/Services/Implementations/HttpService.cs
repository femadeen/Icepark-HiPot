using Hipot.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Hipot.Core.Services.Implementations;

public class HttpService : IHttpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpService> _logger;

    public HttpService(HttpClient httpClient, ILogger<HttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetAsync(string url)
    {
        try
        {
            return await _httpClient.GetStringAsync(url);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP GET request failed for URL: {Url}", url);
            throw;
        }
    }

    public async Task<string> PostAsync(string url, string xmlString)
    {
        try
        {
            var content = new StringContent(xmlString, Encoding.UTF8, "text/xml");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP POST request failed for URL: {Url}", url);
            throw;
        }
    }
}