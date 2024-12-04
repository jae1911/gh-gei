using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using OctoshiftCLI.Contracts;

namespace OctoshiftCLI.Services;

public class GlClient
{
    private const int DEFAULT_PAGE_SIZE = 100;
    private readonly HttpClient _httpClient;
    private readonly OctoLogger _octoLogger;
    private readonly RetryPolicy _retryPolicy;

    public GlClient(OctoLogger octoLogger, HttpClient httpClient, RetryPolicy retryPolicy, IVersionProvider versionProvider, string token) : 
        this(octoLogger, httpClient, retryPolicy, versionProvider)
    {
        if (_httpClient != null)
        {
            var authCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{token}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer OAUTH-TOKEN", authCredentials);
        }
    }

    public GlClient(OctoLogger octoLogger, HttpClient httpClient, RetryPolicy retryPolicy,
        IVersionProvider versionProvider)
    {
        _octoLogger = octoLogger;
        _httpClient = httpClient;
        _retryPolicy = retryPolicy;

        if (_httpClient != null)
        {
            _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("OctoshiftCLI", versionProvider?.GetCurrentVersion()));
            if (versionProvider?.GetVersionComments() is { } comments)
            {
                _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(comments));
            }
        }
    }
}
