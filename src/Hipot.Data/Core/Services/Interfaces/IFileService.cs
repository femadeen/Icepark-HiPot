namespace Hipot.Core.Services.Interfaces
{
    public interface IFileService
    {
        string ScriptPath { get; }
        Task<string> GetScriptContent(string fileName);
    }
}