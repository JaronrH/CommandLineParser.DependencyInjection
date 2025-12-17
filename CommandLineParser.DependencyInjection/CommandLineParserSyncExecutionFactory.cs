using CommandLine;
using CommandLineParser.DependencyInjection.Exceptions;
using CommandLineParser.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;


namespace CommandLineParser.DependencyInjection;

/// <summary>
/// Default Factory that uses <see cref="IServiceProvider"/> to get the appropriate async service to execute command line parser results synchronously.
/// </summary>
public class CommandLineParserSyncExecutionFactory<TResult>(IServiceProvider serviceProvider) : ICommandLineParserSyncExecutionFactory<TResult>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Type ExecuteCommandLineOptionsInterfaceType = typeof(IExecuteCommandLineOptions<,>);

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
    /// <returns>Result</returns>
    /// <exception cref="NoExecuteCommandLineServiceFoundException">Exception thrown when the Command Line Parser was able to get the Options but there was no service found to handle it.</exception>
    public virtual (TResult? Result, bool Handled) ExecuteCommand(string[] args, Type optionsType, object options)
    {
        var serviceInfo = OptionTypeToServiceType.GetOrAdd(optionsType, t =>
        {
            var type = ExecuteCommandLineOptionsInterfaceType.MakeGenericType(optionsType, typeof(TResult));
            var methodType = type.GetMethod("Execute");
            return (type, methodType);
        });
        var executingService = serviceProvider.GetService(serviceInfo.ServiceType);
        return executingService != null && serviceInfo.ExecuteMethodInfo != null
            ? ((TResult)serviceInfo.ExecuteMethodInfo.Invoke(executingService, [options]), true)
            : (default, false);
    }

    /// <summary>
    /// Execute parsing failure asynchronously.
    /// </summary>
    /// <param name="args">Original Arguments</param>
    /// <param name="errors">Command Line Parsing Errors</param>
    /// <returns>Result, if handled</returns>
    public virtual (TResult? Result, bool Handled) ExecuteParsingFailure(string[] args, IEnumerable<Error> errors)
    {
        var service = serviceProvider.GetService<IExecuteParsingFailure<TResult>>(); // Only create when needed
        return service == null
            ? (default, false)
            : (service.Execute(args, errors), true);
    }
}