namespace Play.Common.Settings;

public class ServiceSettings
{
    /// <summary>
    /// The service name of the microservice
    /// </summary>
    public string ServiceName { get; init; }

    /// <summary>
    /// The authority that the microservice will demand to generate acess tokens from.
    /// </summary>
    public string Authority { get; init; }
}
