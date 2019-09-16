using System.Collections.Generic;
using CommandLine;
using CommandLineParser.DependencyInjection.Interfaces;

namespace CommandLineParser.DependencyInjection.Tests.ExecuteOptions
{
    class ExecuteParsingFailure : IExecuteParsingFailure<string>
    {
        #region Implementation of IExecuteParsingFailure<out string>

        /// <summary>
        /// Execute Command Synchronously.
        /// </summary>
        /// <param name="args">Arguments that were passed into the parser.</param>
        /// <param name="errors">Errors as reported from the parser.</param>
        /// <returns>Result</returns>
        public string Execute(string[] args, IEnumerable<Error> errors)
        {
            return $"Unable to parse \"{string.Join(' ', args)}\".";
        }

        #endregion
    }
}