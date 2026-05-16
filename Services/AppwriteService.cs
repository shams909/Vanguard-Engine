using Appwrite;
using Microsoft.Extensions.Configuration;

namespace Vanguard_Engine.Services;

public interface IAppwriteService
{
    Client GetClient();
    string DatabaseId { get; }
    string ProjectId { get; }
    string Endpoint { get; }
}

public class AppwriteService : IAppwriteService
{
    private readonly Client _client;
    public string DatabaseId { get; }
    public string ProjectId { get; }
    public string Endpoint { get; }

    public AppwriteService(IConfiguration configuration)
    {
        Endpoint = configuration["Appwrite:Endpoint"] ?? throw new ArgumentNullException("Appwrite:Endpoint is missing");
        ProjectId = configuration["Appwrite:ProjectId"] ?? throw new ArgumentNullException("Appwrite:ProjectId is missing");
        DatabaseId = configuration["Appwrite:DatabaseId"] ?? throw new ArgumentNullException("Appwrite:DatabaseId is missing");
        var apiKey = configuration["Appwrite:ApiKey"];

        _client = new Client()
            .SetEndpoint(Endpoint)
            .SetProject(ProjectId)
            .SetKey(apiKey!);
    }

    public Client GetClient() => _client;
}
