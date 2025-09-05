using Hipot.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Hipot.Core.Services.Implementations.XML
{
    public class XmlFileService : IFileService
    {
        public string ScriptPath { get; private set; }
        private readonly ILogger<XmlFileService> _logger;

        public XmlFileService() : this(MauiProgram.Services.GetService<ILogger<XmlFileService>>()) {}

        public XmlFileService(ILogger<XmlFileService> logger)
        {
            _logger = logger;
            _logger.LogInformation("XmlFileService constructor started.");

            ScriptPath = Path.Combine(FileSystem.AppDataDirectory, "wwwroot", "tscripts");
            _logger.LogInformation("ScriptPath: {ScriptPath}", ScriptPath);

            EnsureDirectories();
#if DEBUG
            try
            {
                CopyScriptsIfDebug();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CopyScriptsIfDebug. This is likely the cause of the startup error.");
                throw; // Re-throw to ensure the app still fails, but now with logging.
            }
#endif
            _logger.LogInformation("XmlFileService constructor finished.");
        }

        public void EnsureDirectories()
        {
            if (!Directory.Exists(ScriptPath))
            {
                _logger.LogInformation("Creating directory: {ScriptPath}", ScriptPath);
                Directory.CreateDirectory(ScriptPath);
            }
        }

        public string GetProjectRoot()
        {
            var dir = AppContext.BaseDirectory;
            _logger.LogInformation("BaseDirectory: {BaseDirectory}", dir);

            while (!string.IsNullOrEmpty(dir) &&
                   !File.Exists(Path.Combine(dir, "Hipot.csproj")))
            {
                dir = Path.GetDirectoryName(dir);
            }

            if (string.IsNullOrEmpty(dir))
            {
                _logger.LogError("Could not locate project root containing Hipot.csproj starting from {BaseDirectory}.", AppContext.BaseDirectory);
                throw new DirectoryNotFoundException("Could not locate project root.");
            }

            _logger.LogInformation("ProjectRoot found: {ProjectRoot}", dir);
            return dir;
        }

        public void CopyScriptsIfDebug()
        {
            var projectRoot = GetProjectRoot();
            var devScriptPath = Path.Combine(projectRoot, "wwwroot", "tscripts");
            _logger.LogInformation("Dev script path: {devScriptPath}", devScriptPath);

            if (Directory.Exists(devScriptPath))
            {
                _logger.LogInformation("Dev script path exists. Copying files...");
                foreach (var file in Directory.GetFiles(devScriptPath))
                {
                    var destFile = Path.Combine(ScriptPath, Path.GetFileName(file));
                    if (!File.Exists(destFile))
                    {
                        _logger.LogInformation("Copying {file} to {destFile}", file, destFile);
                        File.Copy(file, destFile);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Development script path not found, skipping script copy: {devScriptPath}", devScriptPath);
            }
        }
    }
}
