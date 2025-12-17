namespace CommandLineParser.DependencyInjection.Interfaces;

/// <summary>
/// Validate Command Line Options Synchronously.
/// </summary>
/// <typeparam name="TOptions">Command Line Options to Validate</typeparam>
public interface IValidateCommandLineOptions<in TOptions>
    where TOptions : class, ICommandLineOptions
{
    /// <summary>
    /// Validate Options Synchronously.
    /// </summary>
    /// <param name="options">Options to validate.</param>
    /// <returns>Validation Result</returns>
    bool Validate(TOptions options);
}