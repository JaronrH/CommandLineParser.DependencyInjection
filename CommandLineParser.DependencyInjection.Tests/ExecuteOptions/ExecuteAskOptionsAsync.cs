using System.Threading;
using System.Threading.Tasks;
using CommandLineParser.DependencyInjection.Interfaces;
using CommandLineParser.DependencyInjection.Tests.Options;
using CommandLineParser.DependencyInjection.Tests.Services;

namespace CommandLineParser.DependencyInjection.Tests.ExecuteOptions;

class ExecuteAskOptionsAsync(DoYouLikeService doYouLikeService)
    : IExecuteCommandLineOptionsAsync<AskOptionsAsync, string>
{
    #region Implementation of IExecuteCommandLineOptionsAsync<in AskOptionsAsync,string>

    /// <summary>
    /// Execute Command Asynchronously.
    /// </summary>
    /// <param name="options">Command Line Options</param>
    /// <param name="ctx">Cancellation Token</param>
    /// <returns>Result</returns>
    public Task<string> ExecuteAsync(AskOptionsAsync options, CancellationToken ctx = default) =>
        Task.FromResult(doYouLikeService.DoILikeThis(options.DoYouLike, options.Like, true));

    #endregion
}