using Hipot.Core.Repositories.Implementations;
using Hipot.Core.Repositories.Interfaces;
using Hipot.Core.Services;
using Hipot.Core.Services.Implementations;
using Hipot.Core.Services.Implementations.XML;
using Hipot.Core.Services.Interfaces;
using Hipot.Data;
using Hipot.Data.Core.Services;
using Hipot.Data.Core.Services.Implementations.XML;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Hipot.Data.Core.Context;

namespace Hipot;

public static class MauiProgram
{
    public static IServiceProvider Services { get; private set; }

	public static MauiApp CreateMauiApp()
	{
        try
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();

            builder.Services.AddDbContext<HipotDbContext>(options =>
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "hipot.db");
                options.UseSqlite($"Data Source={dbPath}");
            });

            //Repositories
            builder.Services.AddSingleton<ILogEntryRepository, LogEntryRepository>();

            //Services
            builder.Services.AddSingleton<XmlLogService>();
                        builder.Services.AddSingleton<VpdService>();
            builder.Services.AddSingleton<XmlConfigService>();

            builder.Services.AddSingleton<IHttpService, HttpService>();
            builder.Services.AddSingleton<IUserService, XmlUserService>();
                        builder.Services.AddSingleton<IFileService, FileService>();
            builder.Services.AddSingleton<IChannelConfigService, XmlChannelConfigService>();
            builder.Services.AddSingleton<ISerialNumberService, XmlSerialNumberService>();
            builder.Services.AddSingleton<IXmlTestScriptService, XmlTestScriptService>();
            builder.Services.AddSingleton<ITestService, XmlTestService>();
            builder.Services.AddSingleton<IXmlLoaderService, XmlLoaderService>();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<DataService>();
            builder.Services.AddSingleton<MappingService>();
            builder.Services.AddSingleton<SequenceService>();
            builder.Services.AddSingleton<AppState>();
            builder.Services.AddSingleton<SerialPortService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Trace);
#endif

            var app = builder.Build();

            Services = app.Services;
            var logger = Services.GetService<ILogger<App>>();
            logger.LogInformation("Service provider has been built.");


            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<HipotDbContext>();
                db.Database.Migrate();
                logger.LogInformation("Database migration completed.");
            }

            logger.LogInformation("MauiApp created successfully.");
            return app;
        }
        catch (Exception ex)
        {
            // This is the new, more robust catch block.
            // We'll try to log the exception, but if the logger fails, we'll fall back to Console.
            try
            {
                var logger = Services?.GetService<ILogger<App>>();
                if (logger != null)
                {
                    logger.LogCritical(ex, "A critical error occurred during application startup.");
                }
                else
                {
                    Console.WriteLine($"A critical error occurred during application startup: {ex}");
                }
            }
            catch
            {
                // If logging fails, write to console as a last resort.
                Console.WriteLine($"A critical error occurred during application startup, and logging failed: {ex}");
            }

            // It's important to re-throw the exception to ensure the application still fails fast.
            throw;
        }
	}
}