using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wavelength.Catalogue;
using Wavelength.Playback;
using Wavelength.Playlists;
using Wavelength.Royalties;
using Wavelength.SharedKernel;

namespace Wavelength.Api;

/// <summary>Composition root: wires the modules, error handling, health checks and the CDN path.</summary>
public static class WavelengthApp
{
    public static WebApplication Build(WebApplicationOptions options, Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateBuilder(options);
        var seed = builder.Configuration.GetValue("Wavelength:SeedDemoData", builder.Environment.IsDevelopment());

        builder.Services.AddProblemDetails();
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IIntegrationEventPublisher, InProcessEventPublisher>();
        builder.Services
            .AddCatalogueModule(seed)
            .AddPlaybackModule()
            .AddPlaylistsModule(seed)
            .AddRoyaltiesModule(builder.Configuration);

        configure?.Invoke(builder);

        var app = builder.Build();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseCdnStaticFiles();

        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

        app.MapCatalogueEndpoints();
        app.MapPlaybackEndpoints();
        app.MapPlaylistsEndpoints();
        app.MapRoyaltiesEndpoints();
        return app;
    }
}
