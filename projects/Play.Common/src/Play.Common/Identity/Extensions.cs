using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace Play.Common.Identity;

/// <summary>
/// With this extension class, you can simplify the setup of JWT Bearer authentication for
/// all microservices that need to configure authentication. Notice that options were configured in
/// ConfigureJwtBearerOptions. There's no need re-specify options.
/// </summary>
public static class Extensions
{
    // This method chains additional configuration methods to further customize the authentication setup if needed.
    public static AuthenticationBuilder AddJwtBearerAuthentication(this IServiceCollection services)
	{
		return services.ConfigureOptions<ConfigureJwtBearerOptions>()
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer();
	}
}
