using System.Threading;
using System.Threading.Tasks;
using CommandLineParser.DependencyInjection.Exceptions;

namespace CommandLineParser.DependencyInjection.Interfaces;

/// <summary>
/// Command Line Options Validator that supports both Synchronous and Asynchronous validation.
/// </summary>
public interface ICommandLineOptionsValidator
{
    /// <summary>
    /// Validate Options Synchronously.
    /// </summary>
    /// <remarks>Prefers Synchronous validators but looks for Asynchronous validators as a fallback.</remarks>
    /// <param name="options">Options to validate.</param>
    /// <returns>Validation Result</returns>
    /// <exception cref="CommandLineOptionsValidationException">Thrown when the validator has an exception.</exception>
    bool Validate(ICommandLineOptions options);

    /// <summary>
    /// Validate Options Asynchronously.
    /// </summary>
    /// <remarks>Prefers Asynchronous validators but looks for Synchronous validators as a fallback.</remarks>
    /// <param name="options">Options to validate.</param>
    /// <param name="ctx">Cancellation Token</param>
    /// <returns>Validation Result</returns>
    /// <exception cref="CommandLineOptionsValidationException">Thrown when the validator has an exception.</exception>
    Task<bool> ValidateAsync(ICommandLineOptions options, CancellationToken ctx);
}