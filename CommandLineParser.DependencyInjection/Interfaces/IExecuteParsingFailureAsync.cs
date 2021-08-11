using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;

namespace CommandLineParser.DependencyInjection.Interfaces
{
    /// <summary>
    /// Asynchronously Execute on Parsing Failure.
    /// </summary>
    /// <typeparam name="TResult">Results</typeparam>
    public interface IExecuteParsingFailureAsync<TResult>
    {
        /// <summary>
        /// Execute Command Asynchronously.
        /// </summary>
        /// <param name="args">Arguments that were passed into the parser.</param>
        /// <param name="errors">Errors as reported from the parser.</param>
        /// <returns>Result</returns>
        Task<TResult> ExecuteAsync(string[] args, IEnumerable<Error> errors);
    }
}