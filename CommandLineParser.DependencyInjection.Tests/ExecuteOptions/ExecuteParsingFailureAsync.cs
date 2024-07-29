using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using CommandLineParser.DependencyInjection.Interfaces;

namespace CommandLineParser.DependencyInjection.Tests.ExecuteOptions
{
    class ExecuteParsingFailureAsync : IExecuteParsingFailureAsync<string>
    {
        #region Implementation of IExecuteParsingFailureAsync<string>

        /// <summary>
        /// Execute Command Asynchronously.
        /// </summary>
        /// <param name="args">Arguments that were passed into the parser.</param>
        /// <param name="errors">Errors as reported from the parser.</param>
        /// <returns>Result</returns>
        public Task<string> ExecuteAsync(string[] args, IEnumerable<Error> errors) => Task.FromResult($"Unable to parse \"{string.Join(' ', args)}\" ASYNC.");

        #endregion
    }
}