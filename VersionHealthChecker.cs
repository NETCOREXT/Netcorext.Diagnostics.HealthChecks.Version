using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Netcorext.Diagnostics.HealthChecks.Version;

public class VersionHealthChecker : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public VersionHealthChecker(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var environment = IifStringEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
            var hostName = IifStringEmpty(Environment.GetEnvironmentVariable("HOSTNAME"), Environment.MachineName);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = assemblies.First(t => t.GetName().Name == AppDomain.CurrentDomain.FriendlyName);
            var assemblyName = assembly.GetName().Name;
            var attributes = assembly.GetCustomAttributes().ToArray();
            var attrVersion = attributes.OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            var attrFileVersion = attributes.OfType<AssemblyFileVersionAttribute>().FirstOrDefault();
            var attrConfiguration = attributes.OfType<AssemblyConfigurationAttribute>().FirstOrDefault();
            var attrDescription = attributes.OfType<AssemblyDescriptionAttribute>().FirstOrDefault();

            var d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        { nameof(environment), environment },
                        { nameof(hostName), hostName },
                        { nameof(AssemblyName.Name), assemblyName },
                        { nameof(Assembly.Location), assembly.Location },
                        { nameof(Assembly.ImageRuntimeVersion), assembly.ImageRuntimeVersion },
                        { nameof(AssemblyName.Version), attrVersion?.InformationalVersion },
                        { nameof(FileVersionInfo.FileVersion), attrFileVersion?.Version },
                        { "ConfigVersion", _configuration["ConfigVersion"] },
                        { nameof(AssemblyDescriptionAttribute.Description), attrDescription?.Description },
                        { nameof(AssemblyConfigurationAttribute.Configuration), attrConfiguration?.Configuration }
                    };

            return Task.FromResult(HealthCheckResult.Healthy(data: d));
        }
        catch (Exception e)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(e.Message, e));
        }
    }

    private string IifStringEmpty(string str1, string str2)
    {
        return string.IsNullOrWhiteSpace(str1) ? str2 : str1;
    }
}