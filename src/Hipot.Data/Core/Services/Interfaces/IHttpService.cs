namespace Hipot.Core.Services.Interfaces
{
    public interface IHttpService
    {
        Task<string> GetAsync(string url);
        Task<string> PostAsync(string url, string xmlString);
    }
}
