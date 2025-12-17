using System.Threading;
using System.Threading.Tasks;

namespace CommandLineParser.DependencyInjection.Interfaces;

/// <summary>
/// Validate Command Line Options Asynchronously.
/// </summary>
/// <typeparam name="TOptions">Command Line Options to Validate</typeparam>
public interface IValidateCommandLineOptionsAsync<in TOptions>
    where TOptions : class, ICommandLineOptions
{
    /// <summary>
    /// Validate Options Asynchronously.
    /// </summary>
    /// <param name="options">Options to validate.</param>
    /// <param name="ctx">Cancellation Token</param>
    /// <returns>Validation Result</returns>
    Task<bool> ValidateAsync(TOptions options, CancellationToken ctx);
}