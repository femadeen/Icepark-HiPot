using System.IO;
using System.Threading.Tasks;
using Hipot.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace Hipot.Core.Services.Implementations
{
    public class FileService : IFileService
    {
        public string ScriptPath { get; }
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
            ScriptPath = Path.Combine(FileSystem.AppDataDirectory, "tscript");
            EnsureDirectories();
#if DEBUG
            CopyScriptsIfDebug();
#endif
        }

        public async Task<string> GetScriptContent(string fileName)
        {
            var filePath = Path.Combine(ScriptPath, fileName);
            if (File.Exists(filePath))
            {
                return await File.ReadAllTextAsync(filePath);
            }
            else
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync($"tscript/{fileName}");
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
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
            var devScriptPath = Path.Combine(projectRoot, "Resources", "tscript");
            _logger.LogInformation("Dev script path: {devScriptPath}", devScriptPath);

            if (Directory.Exists(devScriptPath))
            {
                _logger.LogInformation("Dev script path exists. Copying files...");
                foreach (var file in Directory.GetFiles(devScriptPath))
                {
                    var destFile = Path.Combine(ScriptPath, Path.GetFileName(file));
                    var sourceWriteTime = File.GetLastWriteTimeUtc(file);
                    var destWriteTime = File.Exists(destFile) ? File.GetLastWriteTimeUtc(destFile) : DateTime.MinValue;

                    if (sourceWriteTime > destWriteTime)
                    {
                        _logger.LogInformation("Copying {file} to {destFile}", file, destFile);
                        File.Copy(file, destFile, true);
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