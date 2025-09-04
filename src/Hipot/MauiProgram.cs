using Hipot.Core.Context;
using Hipot.Core.Repositories.Implementations;
using Hipot.Core.Repositories.Interfaces;
using Hipot.Core.Services.Implementations;
using Hipot.Core.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hipot;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

        builder.Services.AddDbContext<HipotDbContext>(options =>
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "hipot.db");
            options.UseSqlite($"Data Source={dbPath}");
        });

        //Repositories
        builder.Services.AddScoped<ILogEntryRepository, LogEntryRepository>();

        //Services
        builder.Services.AddScoped<ILogService, LogService>();
        builder.Services.AddScoped<IHttpService, HttpService>();
        builder.Services.AddHttpClient();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HipotDbContext>();
            db.Database.Migrate();
        }

        return app;
    }
}
