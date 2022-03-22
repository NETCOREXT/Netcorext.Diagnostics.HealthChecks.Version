using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Netcorext.Diagnostics.HealthChecks.Version;

namespace Microsoft.Extensions.DependencyInjection;

public static class HealthChecksBuilderExtensions
{
    private const string NAME = "Version";

    public static IHealthChecksBuilder AddVersion(this IHealthChecksBuilder builder, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(name ?? NAME,
                                                       provider => new VersionHealthChecker(provider.GetRequiredService<IConfiguration>()),
                                                       failureStatus,
                                                       tags,
                                                       timeout));
    }
}