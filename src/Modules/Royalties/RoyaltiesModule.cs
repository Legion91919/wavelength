using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Wavelength.Royalties.Application;
using Wavelength.Royalties.Contracts;
using Wavelength.Royalties.Domain;
using Wavelength.SharedKernel;

namespace Wavelength.Royalties;

/// <summary>Royalties module: the append-only play ledger and label statements.</summary>
public static class RoyaltiesModule
{
    public static IServiceCollection AddRoyaltiesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RoyaltyOptions>()
            .Bind(configuration.GetSection(RoyaltyOptions.Section))
            .Validate(o => o.RatePerPlay > 0m && o.Currency.Length == 3, "Royalties:RatePerPlay must be positive and Royalties:Currency a 3-letter code.")
            .ValidateOnStart();
        services.AddSingleton<RoyaltyLedger>();
        services.AddSingleton<StatementService>();
        services.AddSingleton<IIntegrationEventConsumer<TrackPlayQualified>, QualifiedPlayHandler>();
        services.AddHealthChecks().AddCheck("royalty-ledger", () => HealthCheckResult.Healthy("ledger writable"), ["ready"]);
        return services;
    }

    public static IEndpointRouteBuilder MapRoyaltiesEndpoints(this IEndpointRouteBuilder app)
    {
        var labels = app.MapGroup("/api/labels/{labelId}").WithTags("Royalties");

        labels.MapGet("/statement", (string labelId, DateOnly? from, DateOnly? to, StatementService statements) =>
        {
            var (period, problem) = Period(from, to);
            if (problem is not null)
            {
                return problem;
            }

            var statement = statements.Build(labelId, period!);
            return statement is null ? LabelNotFound(labelId) : Results.Ok(statement);
        });

        labels.MapGet("/ledger", (string labelId, DateOnly? from, DateOnly? to, RoyaltyLedger ledger, ICatalogueDirectory directory) =>
        {
            var (period, problem) = Period(from, to);
            if (problem is not null)
            {
                return problem;
            }

            if (directory.FindLabel(labelId) is null)
            {
                return LabelNotFound(labelId);
            }

            return Results.Ok(ledger.Entries(labelId, period!.StartsAt, period.EndsBefore)
                .Select(e => new LedgerEntryResponse(e.Sequence, e.PlayId, e.TrackId, e.QualifiedAt, e.Amount.Amount, e.Amount.Currency))
                .ToList());
        });

        return app;
    }

    private static IResult LabelNotFound(string labelId) =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Label not found", detail: $"No label with id '{labelId}'.");

    private static (StatementPeriod? Period, IResult? Problem) Period(DateOnly? from, DateOnly? to)
    {
        if (from is null || to is null)
        {
            return (null, Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["period"] = ["Both 'from' and 'to' are required, as yyyy-MM-dd (UTC days, inclusive)."],
            }));
        }

        var result = StatementPeriod.Create(from.Value, to.Value);
        return result.Value is null
            ? (null, Results.ValidationProblem(new Dictionary<string, string[]> { ["period"] = [result.Error!] }))
            : (result.Value, null);
    }
}
