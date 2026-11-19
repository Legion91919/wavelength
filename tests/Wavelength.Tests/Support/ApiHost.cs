using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wavelength.Api;

namespace Wavelength.Tests.Support;

/// <summary>Runs the real API in-process on a free port (no NuGet test host needed).</summary>
internal sealed class ApiHost : IAsyncDisposable
{
    public static readonly DateTimeOffset Start = new(2026, 11, 10, 8, 0, 0, TimeSpan.Zero);
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly WebApplication _app;

    private ApiHost(WebApplication app, HttpClient client, ManualTimeProvider clock)
    {
        _app = app;
        Client = client;
        Clock = clock;
    }

    public HttpClient Client { get; }

    public ManualTimeProvider Clock { get; }

    public static async Task<ApiHost> StartAsync()
    {
        var clock = new ManualTimeProvider(Start);
        var app = WavelengthApp.Build(
            new WebApplicationOptions { EnvironmentName = "Development", ContentRootPath = ApiProjectDir() },
            builder =>
            {
                builder.WebHost.UseUrls("http://127.0.0.1:0");
                builder.Logging.ClearProviders();
                builder.Services.AddSingleton<TimeProvider>(clock);
            });
        await app.StartAsync();
        var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
        return new ApiHost(app, client, clock);
    }

    public async Task<HttpResponseMessage> PostPlayAsync(Guid playId, string trackId, int listenedSeconds, string reason = "threshold") =>
        await Client.PostAsJsonAsync("/api/plays", new { playId, trackId, listenedSeconds, reason });

    public async Task<JsonElement> GetJsonAsync(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(Json);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    public static string RepoRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", ".."));

    public static string ApiProjectDir() => Path.Combine(RepoRoot(), "src", "Wavelength.Api");
}
