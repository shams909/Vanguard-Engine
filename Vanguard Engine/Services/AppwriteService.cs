using Appwrite;
using Microsoft.Extensions.Configuration;

namespace Vanguard_Engine.Services;

public interface IAppwriteService
{
    Client GetClient();
    string DatabaseId { get; }
}

public class AppwriteService : IAppwriteService
{
    private readonly Client _client;
    public string DatabaseId { get; }

    public AppwriteService(IConfiguration configuration)
    {
        var endpoint = configuration["Appwrite:Endpoint"];
        var projectId = configuration["Appwrite:ProjectId"];
        var apiKey = configuration["Appwrite:ApiKey"];
        DatabaseId = configuration["Appwrite:DatabaseId"] ?? throw new ArgumentNullException("Appwrite:DatabaseId is missing");

        _client = new Client()
            .SetEndpoint(endpoint!)
            .SetProject(projectId!)
            .SetKey(apiKey!);
    }

    public Client GetClient() => _client;
}
