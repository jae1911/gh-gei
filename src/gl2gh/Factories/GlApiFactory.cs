using OctoshiftCLI;
using OctoshiftCLI.Contracts;
using OctoshiftCLI.Services;

namespace OctoshiftCli.GlToGithub.Factories;

public class GlApiFactory
{
    private readonly OctoLogger _octoLogger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EnvironmentVariableProvider _environmentVariableProvider;
    private readonly IVersionProvider _versionProvider;
    private readonly RetryPolicy _retryPolicy;

    public GlApiFactory(OctoLogger octoLogger, IHttpClientFactory httpClientFactory,
        EnvironmentVariableProvider environmentVariableProvider, IVersionProvider versionProvider,
        RetryPolicy retryPolicy)
    {
        _octoLogger = octoLogger;
        _httpClientFactory = httpClientFactory;
        _environmentVariableProvider = environmentVariableProvider;
        _versionProvider = versionProvider;
        _retryPolicy = retryPolicy;
    }
    
    // TODO: glapi create
}
