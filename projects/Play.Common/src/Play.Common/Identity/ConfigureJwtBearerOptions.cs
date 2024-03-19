using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Play.Common.Settings;

namespace Play.Common.Identity;

/// <summary>
/// This class responsible for configuring JWT Bearer authentication options based on the
/// provided configuration settings, ensuring that the authentication middleware is properly
/// configured for your ASP.NET Core application.
/// </summary>
public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    /// <summary>
    /// Have an instance of configuration to get Selections respectively.
    /// </summary>
    private readonly IConfiguration configuration;

    public ConfigureJwtBearerOptions(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    // You're able to generalize any microservice for Identity Service configurations. Recall that the audience is the
    // name of the service, and the authority is where the microservices will request to generate access tokens from.
    // Ensure to also include MapInBoundClaims to false to avoid legacy errors conflict with newer external identity providers
    // because they might be using modern claims that this service doesn't use.
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

            options.Authority = serviceSettings.Authority;
            options.Audience = serviceSettings.ServiceName;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                NameClaimType = "name",
                RoleClaimType = "role"
            };
        }
    }

    // For this method just call the previous signature method.
    public void Configure(JwtBearerOptions options)
    {
        Configure(Options.DefaultName, options);
    }
}