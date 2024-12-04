﻿using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using OctoshiftCLI;
using OctoshiftCLI.Extensions;
using OctoshiftCLI.Factories;
using OctoshiftCLI.Services;

[assembly: InternalsVisibleTo("OctoshiftCli.Tests")]

namespace OctoshiftCli.GlToGithub
{
    public static class Program
    {
        private static readonly OctoLogger Logger = new();

        public static async Task Main(string[] args)
        {
            Logger.LogDebug("Execution started");

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton(Logger)
                .AddSingleton<EnvironmentVariableProvider>()
                .AddSingleton<RetryPolicy>()
                .AddSingleton<GithubStatusApi>()
                .AddSingleton<VersionChecker>()
                .AddSingleton<BasicHttpClient>()
                .AddSingleton<HttpDownloadServiceFactory>()
                .AddSingleton<FileSystemProvider>()
                .AddSingleton<DateTimeProvider>()
                .AddSingleton<ConfirmationService>()
                .AddHttpClient("NoSSL")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    CheckCertificateRevocationList = false,
                    ServerCertificateCustomValidationCallback = delegate { return true; }
                })
                .Services
                .AddHttpClient("Default");
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var parser = BuildParser(serviceProvider);

            SetContext(parser.Parse(args));

            try
            {
                await GithubStatusCheck(serviceProvider);
            }
            catch (Exception e)
            {
                Logger.LogWarning("Could not check GitHub availability from githubstatus.com. See See https://www.githubstatus.com for details.");
                Logger.LogVerbose(e.ToString());
            }

            try
            {
                await LatestVersionCheck(serviceProvider);
            }
            catch (Exception e)
            {
                Logger.LogWarning("Could not retreive latest gl2gh version from github.com, please ensure you are using the latest version by running: gh extension upgrade gl2gh.");
                Logger.LogVerbose(e.ToString());
            }

            await parser.InvokeAsync(args);
        }

        private static void SetContext(ParseResult parseResult)
        {
            CliContext.RootCommand = "gl2gh";
            CliContext.ExecutingCommand = parseResult.CommandResult.Command.Name;
        }

        private static async Task GithubStatusCheck(ServiceProvider serviceProvider)
        {
            var envProvider = serviceProvider.GetRequiredService<EnvironmentVariableProvider>();

            if (envProvider.SkipStatusCheck().ToUpperInvariant() is "TRUE" or "1")
            {
                Logger.LogInformation("Skipped GitHub status check due to GEI_SKIP_STATUS_CHECK environment variable");
                return;
            }
            
            var githubStatusApi = serviceProvider.GetRequiredService<GithubStatusApi>();

            if (await githubStatusApi.GetUnresolvedIncidentsCount() > 0)
            {
                Logger.LogWarning("GitHub is currently experiencing availability issues. See https://www.githubstatus.com for details.");
            }
        }

        private static async Task LatestVersionCheck(ServiceProvider serviceProvider)
        {
            var envProvider = serviceProvider.GetRequiredService<EnvironmentVariableProvider>();

            if (envProvider.SkipVersionCheck().ToUpperInvariant() is "TRUE" or "1")
            {
                Logger.LogInformation("Skipped latest version check due to GEI_SKIP_VERSION_CHECK environment variable");
                return;
            }
            
            var versionChecker = serviceProvider.GetRequiredService<VersionChecker>();

            if (await versionChecker.IsLatest())
            {
                Logger.LogInformation($"You are using an up-to-date version of the gl2gh extension [v{versionChecker.GetCurrentVersion()}]");
            }
            else
            {
                Logger.LogWarning($"You are running an old version of the gl2gh extension [v{versionChecker.GetCurrentVersion()}]. The latest version is v{versionChecker.GetLatestVersion()}.");
                Logger.LogWarning("Please update by running: gh extension upgrade gl2gh");
            }
        }

        private static Parser BuildParser(ServiceProvider serviceProvider)
        {
            var root = new RootCommand("CLI for GitLab importer.")
                .AddCommands(serviceProvider);
            var commandLineBuilder = new CommandLineBuilder(root);
            
            return commandLineBuilder
                .UseDefaults()
                .UseExceptionHandler((ex, _) =>
                {
                    Logger.LogError(ex);
                    Environment.ExitCode = 1;
                }, 1)
                .Build();
        }
    }
}

