using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLineParser.DependencyInjection.Exceptions;

namespace CommandLineParser.DependencyInjection.Interfaces;

/// <summary>
/// Factory used to execute command line parser results asynchronously.
/// </summary>
/// <typeparam name="TResult">Result Type</typeparam>
public interface ICommandLineParserAsyncExecutionFactory<TResult>
{
    /// <summary>
    /// Priority.  Higher number is higher priority.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Execute requested command asynchronously.
    /// </summary>
    /// <param name="args">Original Arguments</param>
    /// <param name="optionsType">Options Type</param>
    /// <param name="options">Options</param>
    /// <param name="ctx">Cancellation Token</param>
    /// <returns>Result</returns>
    /// <exception cref="NoExecuteCommandLineServiceFoundException">Exception thrown when the Command Line Parser was able to get the Options but there was no service found to handle it.</exception>
    Task<(TResult? Result, bool Handled)> ExecuteCommandAsync(string[] args, Type optionsType, object options, CancellationToken ctx);

    /// <summary>
    /// Execute parsing failure asynchronously.
    /// </summary>
    /// <param name="args">Original Arguments</param>
    /// <param name="errors">Command Line Parsing Errors</param>
    /// <param name="ctx">Cancellation Token</param>
    /// <returns>Result, if handled</returns>
    Task<(TResult? Result, bool Handled)> ExecuteParsingFailureAsync(string[] args, IEnumerable<Error> errors, CancellationToken ctx);
}