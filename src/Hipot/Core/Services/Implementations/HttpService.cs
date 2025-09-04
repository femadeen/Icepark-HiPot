using Hipot.Core.Services.Interfaces;
using System.Text;

namespace Hipot.Core.Services.Implementations;

public class HttpService : IHttpService
{
    private readonly HttpClient _httpClient;

    public HttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetAsync(string url)
    {
        try
        {
            return await _httpClient.GetStringAsync(url);
        }
        catch (HttpRequestException ex)
        {
            // log this exception.
            return ex.Message;
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
            // log this exception.
            return ex.Message;
        }
    }
}