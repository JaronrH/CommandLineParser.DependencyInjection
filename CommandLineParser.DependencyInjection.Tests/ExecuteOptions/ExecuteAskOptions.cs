using CommandLineParser.DependencyInjection.Interfaces;
using CommandLineParser.DependencyInjection.Tests.Options;
using CommandLineParser.DependencyInjection.Tests.Services;

namespace CommandLineParser.DependencyInjection.Tests.ExecuteOptions;

class ExecuteAskOptions(DoYouLikeService doYouLikeService) : IExecuteCommandLineOptions<AskOptions, string>
{
    #region Implementation of IExecuteCommandLineOptions<in AskOptions,out string>

    /// <summary>
    /// Execute Command Synchronously.
    /// </summary>
    /// <param name="options">Command Line Options</param>
    /// <returns>Result</returns>
    public string Execute(AskOptions options) => doYouLikeService.DoILikeThis(options.DoYouLike, options.Like, false);

    #endregion
}
