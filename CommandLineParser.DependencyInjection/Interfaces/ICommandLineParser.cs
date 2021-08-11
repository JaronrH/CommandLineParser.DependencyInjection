using System;
using System.Threading.Tasks;
using CommandLine;

namespace CommandLineParser.DependencyInjection.Interfaces
{
    /// <summary>
    /// Wrapper for the <see cref="Parser"/> which leverages <see cref="IServiceProvider"/> to get the Options (<see cref="ICommandLineOptions"/>)
    /// and execute either the corresponding <see cref="IExecuteCommandLineOptions{TCommandLineOptions,TResult}"/> or <see cref="IExecuteParsingFailure{TResult}"/>.
    ///
    /// All services are Singleton's and should only be registered once in the DI container.  <see cref="ICommandLineOptions"/>, while resolved, are not
    /// used from DI but rather just used to get the types from DI to register in the parser.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface ICommandLineParser<TResult>
    {
        /// <summary>
        /// Parse Command Line Arguments using <see cref="Parser"/>.
        /// </summary>
        /// <param name="args">Command Line Arguments.</param>
        /// <param name="configuration">Optional Parser Configuration Action.</param>
        /// <param name="defaultResult">Default Result to return when parser was unable to parse out options.</param>
        /// <param name="allowAsyncImplementations">Fall back to <see cref="IExecuteCommandLineOptionsAsync{TCommandLineOptions,TResult}"/> and/or <see cref="IExecuteParsingFailureAsync{TResult}"/> implementations an run them synchronously when synchronous version are not available?</param>
        /// <returns>Result [code].</returns>
        TResult ParseArguments(string[] args, Action<ParserSettings> configuration = null, TResult defaultResult = default, bool allowAsyncImplementations = true);

        /// <summary>
        /// Parse Command Line Arguments using <see cref="Parser"/>.
        /// </summary>
        /// <param name="args">Command Line Arguments.</param>
        /// <param name="configuration">Optional Parser Configuration Action.</param>
        /// <param name="defaultResult">Default Result to return when parser was unable to parse out options.</param>
        /// <param name="allowSyncImplementations">Fall back to <see cref="IExecuteCommandLineOptions{TCommandLineOptions,TResult}"/> and/or <see cref="IExecuteParsingFailure{TResult}"/> implementations asynchronous version are not available?</param>
        /// <returns>Result [code].</returns>
        Task<TResult> ParseArgumentsAsync(string[] args, Action<ParserSettings> configuration = null, TResult defaultResult = default, bool allowSyncImplementations = true);

        /// <summary>
        /// Parse Command Line Arguments using <see cref="Parser"/>.
        /// </summary>
        /// <param name="args">Command Line Arguments.</param>
        /// <param name="defaultResult">Default Result to return when parser was unable to parse out options.</param>
        /// <param name="allowAsyncImplementations">Fall back to <see cref="IExecuteCommandLineOptionsAsync{TCommandLineOptions,TResult}"/> and/or <see cref="IExecuteParsingFailureAsync{TResult}"/> implementations an run them synchronously when synchronous version are not available?</param>
        /// <returns>Result [code].</returns>
        TResult ParseArguments(string[] args, TResult defaultResult, bool allowAsyncImplementations = true) =>
            ParseArguments(args, null, defaultResult, allowAsyncImplementations);

        /// <summary>
        /// Parse Command Line Arguments using <see cref="Parser"/>.
        /// </summary>
        /// <param name="args">Command Line Arguments.</param>
        /// <param name="defaultResult">Default Result to return when parser was unable to parse out options.</param>
        /// <param name="allowSyncImplementations">Fall back to <see cref="IExecuteCommandLineOptions{TCommandLineOptions,TResult}"/> and/or <see cref="IExecuteParsingFailure{TResult}"/> implementations asynchronous version are not available?</param>
        /// <returns>Result [code].</returns>
        Task<TResult> ParseArgumentsAsync(string[] args, TResult defaultResult,
            bool allowSyncImplementations = true) =>
            ParseArgumentsAsync(args, null, defaultResult, allowSyncImplementations);
    }
}