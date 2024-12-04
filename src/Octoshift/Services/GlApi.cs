using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OctoshiftCLI.Services;

public class GlApi
{
    private readonly GlClient _client;
    private readonly string _glBaseUrl;
    private readonly OctoLogger _log;

    public GlApi(OctoLogger log, GlClient client, string apiBaseUrl)
    {
        _client = client;
        _glBaseUrl = apiBaseUrl;
        _log = log;
    }

    public virtual async Task<string> GetServerVersion()
    {
        var url = $"{_glBaseUrl}/version";
        
        var content = await _client.GetAsync(url);
        
        return (string)JsonObject.Parse(content)["version"];
    }
}
