using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using OctoshiftCLI.Extensions;
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
                .AddSingleton(Logger);
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var rootCommand = new RootCommand("Automated end-to-end GitLab to GitHub migrations.")
                .AddCommands(serviceProvider);

            var commandLineBuilder = new CommandLineBuilder(rootCommand);
            var parser = commandLineBuilder
                .UseDefaults()
                .UseExceptionHandler((ex, _) =>
                {
                    Logger.LogError(ex);
                    Environment.ExitCode = 1;
                }, 1)
                .Build();

            // TODO: set context like bbs2gh?
            
            // TODO: implement statusCheck
            
            // TODO: implement latestversionCheck

            await parser.InvokeAsync(args);
        }
    }
}

