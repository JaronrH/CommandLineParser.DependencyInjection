# CommandLineParser.DependencyInjection
 
This is a simple wrapper around the amazing [commandlineparser/commandline](https://github.com/commandlineparser/commandline) library which adds support for Microsoft's DI with help from [Scrutor](https://github.com/khellang/Scrutor).  This is done by allow the following interfaces to be implemented which DI will find and user for executing the commandlineparser.

|Interface|Description  |
|--|--|
|ICommandLineOptions | This is an empty interface that is used solely for DI to find options! Make a class with [Options](https://github.com/commandlineparser/commandline/wiki/Option-Attribute) just like the CommandLineParser library instructs! |
|ICommandLineParser< TResult >| This is the DI Service you will use to access the [Parser](https://github.com/commandlineparser/commandline/wiki/Getting-Started) and execute the relevant DI Service for IExecuteCommandLineOptions/IExecuteCommandLineOptionsAsync.  It has the ability to run both synchronously or asynchronously. |
|IExecuteCommandLineOptions<TCommandLineOptions, TResult>| Interface that implements Execute(options) synchronously with the Parser's Command Line Options.  |
|IExecuteCommandLineOptionsAsync<TCommandLineOptions, TResult>| Interface that implements ExecuteAsync(options) asynchronously with the Parser's Command Line Options. |
|IExecuteParsingFailure<TResult>| If no Options/Handler found, this is called synchronously  with the executed arguments array and parser errors to handle the inability to handle the command line input arguments. |
|IExecuteParsingFailureAsync< TResult >|If no Options/Handler found, this is called asynchronously with the executed arguments array and parser errors to handle the inability to handle the command line input arguments.   |

## Example

Here is an example of the QuickStart from  [commandlineparser/commandline](https://github.com/commandlineparser/commandline) implemented using DI.  In this case though, we're writing to Microsoft's Logging ILogger that is getting injected using DI instead of writing directly to console!

```
using CommandLine;
using CommandLineParser.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
 
new ServiceCollection() // Create Service Collection
        .AddCommandLineParser(typeof(Options).Assembly) // Add CommandLineParser registrations to DI
        .AddLogging(c => c.AddConsole()) // Add Console Logging
    .BuildServiceProvider() // Build Service Provider
        .GetRequiredService<ICommandLineParser<int>>() // Get Parser Service
            .ParseArguments(args, -1) // Call Parser with Arguments (Will have Options as well as and ExecuteOptions from DI)
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
```


### Unit Tests

***See the Test's project for example.***  

The Tests project has 3 folders:
 - Options: These are the CommandLineOptions, exactly how the [commandlineparser/commandline](https://github.com/commandlineparser/commandline)  library defines them, with the ICommandLineOptions interface added for DI discovering.  *The only difference is that one is used for calling async instead of sync in tests.*
 - ExecuteAskOptions: These are both Sync and Async dummy implementations of services that handle the AskOptions as well as handle Parsing failures.
 - Services: Dummy service that returns a string based on the arguments provided.

