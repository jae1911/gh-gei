using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using OctoshiftCLI.Contracts;
using OctoshiftCLI.Extensions;

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

    public virtual async Task<string> GetAsync(string url) => await _retryPolicy.Retry(async () => await SendAsync(HttpMethod.Get, url));
    
    public virtual async Task<string> PostAsync(string url, object body) => await SendAsync(HttpMethod.Post, url, body);

    private async Task<string> SendAsync(HttpMethod method, string url, object body = null)
    {
        _octoLogger.LogVerbose($"HTTP {method}: {url}");

        if (body != null)
        {
            _octoLogger.LogVerbose($"HTTP BODY: {body.ToJson()}");
        }
        
        using var payload = body?.ToJson().ToStringContent();
        var response = method.ToString() switch
        {
            "GET" => await _httpClient.GetAsync(url),
            "POST" => await _httpClient.PostAsync(url, payload),
            _ => throw new ArgumentOutOfRangeException($"{method} is not supported")
        };
        
        var content = await response.Content.ReadAsStringAsync();
        _octoLogger.LogVerbose($"RESPONSE ({response.StatusCode}): {content}");
        
        response.EnsureSuccessStatusCode();

        return content;
    }
    
}
