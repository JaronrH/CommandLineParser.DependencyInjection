using CommandLine;
using CommandLineParser.DependencyInjection.Exceptions;
using CommandLineParser.DependencyInjection.Extensions;
using CommandLineParser.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace CommandLineParser.DependencyInjection;

/// <summary>
/// Default Factory that uses <see cref="IServiceProvider"/> to get the appropriate async service to execute command line parser results asynchronously.
/// </summary>
public class CommandLineParserAsyncExecutionFactory<TResult>(IServiceProvider serviceProvider) : ICommandLineParserAsyncExecutionFactory<TResult>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Type ExecuteCommandLineOptionsAsyncInterfaceType = typeof(IExecuteCommandLineOptionsAsync<,>);

    /// <summary>
    /// Options Type to Service Type Map
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<Type, (Type ServiceType, MethodInfo ExecuteMethodInfo)> OptionTypeToServiceType = new();

    /// <summary>
    /// Priority.  Higher number is higher priority.
    /// </summary>
    public int Priority => 0;

    /// <summary>
    /// Execute requested command asynchronously.
    /// </summary>
    /// <param name="args">Original Arguments</param>
    /// <param name="optionsType">Options Type</param>
    /// <param name="options">Options</param>
    /// <param name="ctx">Cancellation Token</param>
    /// <returns>Result</returns>
    /// <exception cref="NoExecuteCommandLineServiceFoundException">Exception thrown when the Command Line Parser was able to get the Options but there was no service found to handle it.</exception>
    public virtual async Task<(TResult? Result, bool Handled)> ExecuteCommandAsync(string[] args, Type optionsType, object options, CancellationToken ctx)
    {
        var serviceInfo = OptionTypeToServiceType.GetOrAdd(optionsType, t =>
        {
            var type = ExecuteCommandLineOptionsAsyncInterfaceType.MakeGenericType(optionsType, typeof(TResult));
            var methodType = type.GetMethod("ExecuteAsync");
            return (type, methodType);
        });
        var executingService = serviceProvider.GetService(serviceInfo.ServiceType);
        return executingService != null && serviceInfo.ExecuteMethodInfo != null
            ? (await serviceInfo.ExecuteMethodInfo.InvokeAsync<TResult>(executingService, new[] { options, ctx }), true) 
            : (default, false);
    }

    /// <summary>
    /// Execute parsing failure asynchronously.
    /// </summary>
    /// <param name="args">Original Arguments</param>
    /// <param name="errors">Command Line Parsing Errors</param>
    /// <param name="ctx">Cancellation Token</param>
    /// <returns>Result, if handled</returns>
    public virtual async Task<(TResult? Result, bool Handled)> ExecuteParsingFailureAsync(string[] args, IEnumerable<Error> errors, CancellationToken ctx)
    {
        var service = serviceProvider.GetService<IExecuteParsingFailureAsync<TResult>>(); // Only create when needed
        return service == null
            ? (default, false)
            : (await service.ExecuteAsync(args, errors, ctx), true);
    }
}