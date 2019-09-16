using System.Collections.Generic;
using CommandLine;

namespace CommandLineParser.DependencyInjection.Interfaces
{
    /// <summary>
    /// Synchronously Execute on Parsing Failure.
    /// </summary>
    /// <typeparam name="TResult">Results</typeparam>
    public interface IExecuteParsingFailure<out TResult>
    {
        /// <summary>
        /// Execute Command Synchronously.
        /// </summary>
        /// <param name="args">Arguments that were passed into the parser.</param>
        /// <param name="errors">Errors as reported from the parser.</param>
        /// <returns>Result</returns>
        TResult Execute(string[] args, IEnumerable<Error> errors);
    }
}