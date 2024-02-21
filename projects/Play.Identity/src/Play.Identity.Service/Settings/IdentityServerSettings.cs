using System;
using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace Play.Identity.Service.Settings;

public class IdentityServerSettings
{
    /// <summary>
    /// Collection of ApiScopes that will be used for Identity Server.
    /// </summary>
    public IReadOnlyCollection<ApiScope> ApiScopes { get; init; } = Array.Empty<ApiScope>();

    /// <summary>
    /// Collection of clients.
    /// </summary>
    public IReadOnlyCollection<Client> Clients { get; init; } = Array.Empty<Client>();

    /// <summary>
    /// Specify other types of scopes for Identity Server.
    /// </summary>
    public IReadOnlyCollection<IdentityResource> identityResources => new IdentityResource[]
    {
        new IdentityResources.OpenId()
    };
}
