using System.Threading.Tasks;
using CommandLineParser.DependencyInjection.Interfaces;
using CommandLineParser.DependencyInjection.Tests.Options;
using CommandLineParser.DependencyInjection.Tests.Services;

namespace CommandLineParser.DependencyInjection.Tests.ExecuteOptions
{
    class ExecuteAskOptionsAsync : IExecuteCommandLineOptionsAsync<AskOptionsAsync, string>
    {
        private readonly DoYouLikeService _doYouLikeService;

        public ExecuteAskOptionsAsync(DoYouLikeService doYouLikeService)
        {
            _doYouLikeService = doYouLikeService;
        }

        #region Implementation of IExecuteCommandLineOptionsAsync<in AskOptionsAsync,string>

        /// <summary>
        /// Execute Command Asynchronously.
        /// </summary>
        /// <param name="options">Command Line Options</param>
        /// <returns>Result</returns>
        public Task<string> ExecuteAsync(AskOptionsAsync options)
        {
            return Task.FromResult(_doYouLikeService.DoILikeThis(options.DoYouLike, options.Like, true));
        }

        #endregion
    }
}