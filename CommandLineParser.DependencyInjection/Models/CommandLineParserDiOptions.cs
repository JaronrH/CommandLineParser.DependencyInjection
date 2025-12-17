using CommandLineParser.DependencyInjection.Interfaces;

namespace CommandLineParser.DependencyInjection.Models;

/// <summary>
/// Command Line Parser DI Options
/// </summary>
public class CommandLineParserDiOptions
{
    /// <summary>
    /// When true, only public types will be registered (default).
    /// </summary>
    public bool PublicOnly { get; set; } = true;

    /// <summary>
    /// Always include the calling assembly when scanning for services even if assemblies are provided.  True by default.
    /// </summary>
    public bool AlwaysIncludeCallingAssembly { get; set; } = true;

    /// <summary>
    /// Scan and import services for <see cref="IExecuteCommandLineOptionsAsync{TCommandLineOptions,TResult}"/>, <see cref="IExecuteParsingFailureAsync{TResult}"/> & <see cref="IValidateCommandLineOptionsAsync{TOptions}"/> and factories for <see cref="ICommandLineParserAsyncExecutionFactory{TResult}"/>.  True by default.
    /// </summary>
    /// <remarks>Services will be transient while factories will be singleton.</remarks>
    public bool FindAsyncServices { get; set; } = true;

    /// <summary>
    /// Include default <see cref="ICommandLineParserAsyncExecutionFactory{TResult}"/> factory for async execution.  True by default.
    /// </summary>
    public bool IncludeDefaultAsyncFactory { get; set; } = true;

    /// <summary>
    /// Scan and import services for <see cref="IExecuteCommandLineOptions{TCommandLineOptions,TResult}"/>, <see cref="IExecuteParsingFailure{TResult}"/> & <see cref="IValidateCommandLineOptions{TOptions}"/> and factories for <see cref="ICommandLineParserSyncExecutionFactory{TResult}"/>.  True by default.
    /// </summary>
    /// <remarks>Services will be transient while factories will be singleton.</remarks>
    public bool FindSyncServices { get; set; } = true;

    /// <summary>
    /// Include default <see cref="ICommandLineParserSyncExecutionFactory{TResult}"/> factory for async execution.  True by default.
    /// </summary>
    public bool IncludeDefaultSyncFactory { get; set; } = true;

    /// <summary>
    /// Include default <see cref="ICommandLineOptionsValidator"/> validator for options validation.  True by default.
    /// </summary>
    /// <remarks>Turn off if not using validators!</remarks>
    public bool IncludeDefaultOptionsValidator { get; set; } = true;
}

