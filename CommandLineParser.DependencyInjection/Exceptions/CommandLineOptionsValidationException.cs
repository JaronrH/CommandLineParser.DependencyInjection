using System;
using CommandLineParser.DependencyInjection.Interfaces;

namespace CommandLineParser.DependencyInjection.Exceptions;

/// <summary>
/// Command Line Options Validation Exception
/// </summary>
/// <param name="options">Options that are invalid.</param>
/// <param name="innerException">Exception that was thrown</param>
public class CommandLineOptionsValidationException(ICommandLineOptions options, Exception? innerException = null)
    : Exception($"Command Line Options '{options.GetType().Name}' are invalid.", innerException)
{
    /// <summary>
    /// Invalid Options
    /// </summary>
    public ICommandLineOptions Options { get; } = options;
}