using System;
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
    public interface ICommandLineParser<out TResult>
    {
        /// <summary>
        /// Parse Command Line Arguments using <see cref="Parser"/>.
        /// </summary>
        /// <param name="args">Command Line Arguments.</param>
        /// <param name="configuration">Optional Parser Configuration Action.</param>
        /// <returns>Result [code].</returns>
        TResult ParseArguments(string[] args, Action<ParserSettings> configuration = null);
    }
}