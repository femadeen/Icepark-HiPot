namespace Hipot.Core.Services.Interfaces
{
    public interface IFileService
    {
        string ScriptPath { get; }
        string GetProjectRoot();
        void EnsureDirectories();
        void CopyScriptsIfDebug();
    }
}
