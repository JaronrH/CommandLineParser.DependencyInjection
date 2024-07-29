using CommandLine;
using CommandLineParser.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

new ServiceCollection() // Create Service Collection
        .AddCommandLineParser(typeof(Options).Assembly) // Add CommandLineParser registrations to DI
        .AddLogging(c => c.AddConsole()) // Add Console Logging
    .BuildServiceProvider() // Build Service Provider
        .GetRequiredService<ICommandLineParser<int>>() // Get Parser Service
            .ParseArguments(args, -1) // Call Parser with Arguments (Options and ExecuteOptions will be loaded from DI as needed)
    ;

public class Options: ICommandLineOptions
{
    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }
}

public class ExecuteOptions(ILogger<ExecuteOptions> log) : IExecuteCommandLineOptions<Options, int>
{
    #region Implementation of IExecuteCommandLineOptions<in Options,out int>

    /// <summary>
    /// Execute Command Synchronously.
    /// </summary>
    /// <param name="options">Command Line Options</param>
    /// <returns>Result</returns>
    public int Execute(Options options)
    {
        if (options.Verbose)
        {
            log.LogInformation($"Verbose output enabled. Current Arguments: -v {options.Verbose}");
            log.LogWarning("Quick Start Example! App is in Verbose mode!");
        }
        else
        {
            log.LogInformation($"Current Arguments: -v {options.Verbose}");
            log.LogInformation("Quick Start Example!");
        }

        return 0;
    }

    #endregion
}