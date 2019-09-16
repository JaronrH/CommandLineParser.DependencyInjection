using CommandLineParser.DependencyInjection.Interfaces;
using CommandLineParser.DependencyInjection.Tests.Options;
using CommandLineParser.DependencyInjection.Tests.Services;

namespace CommandLineParser.DependencyInjection.Tests.ExecuteOptions
{
    class ExecuteAskOptions : IExecuteCommandLineOptions<AskOptions, string>
    {
        private readonly DoYouLikeService _doYouLikeService;

        public ExecuteAskOptions(DoYouLikeService doYouLikeService)
        {
            _doYouLikeService = doYouLikeService;
        }

        #region Implementation of IExecuteCommandLineOptions<in AskOptions,out string>

        /// <summary>
        /// Execute Command Synchronously.
        /// </summary>
        /// <param name="options">Command Line Options</param>
        /// <returns>Result</returns>
        public string Execute(AskOptions options)
        {
            return _doYouLikeService.DoILikeThis(options.DoYouLike, options.Like);
        }

        #endregion
    }
}