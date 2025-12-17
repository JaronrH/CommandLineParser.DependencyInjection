using CommandLine;
using CommandLineParser.DependencyInjection.Exceptions;
using CommandLineParser.DependencyInjection.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CommandLineParser.DependencyInjection;

public class CommandLineParser<TResult> : ICommandLineParser<TResult>
{
    private readonly Type[] _commandLineOptionTypes;
    private readonly ILogger<CommandLineParser<TResult>>? _log;
    private readonly IEnumerable<ICommandLineParserAsyncExecutionFactory<TResult>> _asyncExecutionFactories;
    private readonly IEnumerable<ICommandLineParserSyncExecutionFactory<TResult>> _syncExecutionFactories;
    private readonly ICommandLineOptionsValidator? _commandLineOptionsValidator;

    public CommandLineParser(IEnumerable<ICommandLineOptions> commandLineOptions, IEnumerable<ICommandLineParserAsyncExecutionFactory<TResult>> asyncExecutionFactories, IEnumerable<ICommandLineParserSyncExecutionFactory<TResult>> syncExecutionFactories, ICommandLineOptionsValidator? commandLineOptionsValidator = null, ILogger<CommandLineParser<TResult>>? log = null)
    {
        _commandLineOptionsValidator = commandLineOptionsValidator;
        _log = log;
        _asyncExecutionFactories = asyncExecutionFactories
            .OrderByDescending(i => i.Priority)
            .ToArray();
        _syncExecutionFactories = syncExecutionFactories
            .OrderByDescending(i => i.Priority)
            .ToArray();
        _commandLineOptionTypes = commandLineOptions.Select(i => i.GetType()).ToArray();

        if (_commandLineOptionTypes.Length == 0)
            log?.LogWarning("No ICommandLineOptions implementations were found. Ensure that you have registered at least one implementation of ICommandLineOptions with the DI container.");
    }

    #region Implementation of ICommandLineParser<TResult>

    /// <summary>
    /// Parse Command Line Arguments using <see cref="Parser"/>.
    /// </summary>
    /// <param name="args">Command Line Arguments.</param>
    /// <param name="configuration">Optional Parser Configuration Action.</param>
    /// <param name="defaultResult">Default Result to return when parser was unable to parse out options.</param>
    /// <param name="allowAsyncImplementations">Fall back to <see cref="IExecuteCommandLineOptionsAsync{TCommandLineOptions,TResult}"/> and/or <see cref="IExecuteParsingFailureAsync{TResult}"/> implementations and run them synchronously when synchronous version are not available?</param>
    /// <returns>Result [code].</returns>
    /// <exception cref="CommandLineOptionsValidationException">Options was found to be invalid (or there was an exception while validating the options)</exception>
    /// <exception cref="NoExecuteCommandLineServiceFoundException">No handler for Command line Options.</exception>
    public TResult? ParseArguments(string[] args, Action<ParserSettings>? configuration = null, TResult? defaultResult = default,
        bool allowAsyncImplementations = true)
    {
        // Create Parser
        using var parser = configuration == null
            ? new Parser()
            : new Parser(configuration);

        // Execute Parser
        var result = _commandLineOptionTypes.Length == 1 && _commandLineOptionTypes.All(i => !i.GetCustomAttributes<VerbAttribute>().Any())
            ? parser.ParseArguments(() => Activator.CreateInstance(_commandLineOptionTypes.First()), args)
            : parser.ParseArguments(args, _commandLineOptionTypes);
        var errors = ((result as NotParsed<object>)?.Errors ?? []).ToArray();
        if (result.Tag == ParserResultType.Parsed)
            _log?.LogInformation("Command Line Arguments Parser determined that the '{Type}' type is to be used for options.", result.TypeInfo.Current.Name);
        else
            _log?.LogError("Command Line Arguments Parser was unable to parse arguments.");

        // Parser Execute successfully?
        if (result.Tag == ParserResultType.Parsed)
        {
            // Validate Options
            if (_commandLineOptionsValidator != null && result.Value is ICommandLineOptions clo && !_commandLineOptionsValidator.Validate(clo))
                throw new CommandLineOptionsValidationException(clo);

            // Get Parsed Value
            if (result is Parsed<object> parsed)
            {
                // Look for Sync Types to execute
                foreach (var syncExecutionFactory in _syncExecutionFactories)
                    try
                    {
                        _log?.LogDebug("Handling options '{Type}' using the Sync Execution Factory '{Factory}'.",
                            result.TypeInfo.Current.Name, syncExecutionFactory.GetType().Name);
                        var executionResult =
                            syncExecutionFactory.ExecuteCommand(args, result.TypeInfo.Current, parsed.Value);
                        if (executionResult.Handled) return executionResult.Result;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        _log?.LogError(e, "Exception thrown while handling options '{Type}' using the Sync Execution Factory '{Factory}'.", result.TypeInfo.Current.Name, syncExecutionFactory.GetType().Name);
                    }

                // Look for Async?
                if (allowAsyncImplementations)
                    foreach (var asyncExecutionFactory in _asyncExecutionFactories)
                        try
                        {
                            _log?.LogDebug("Handling options '{Type}' using the Async Execution Factory '{Factory}'.", result.TypeInfo.Current.Name, asyncExecutionFactory.GetType().Name);
                            var executionResult =
                            AsyncHelper.RunSync(async () => await asyncExecutionFactory.ExecuteCommandAsync(args, result.TypeInfo.Current, parsed.Value, CancellationToken.None));
                            if (executionResult.Handled) return executionResult.Result;
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            _log?.LogError(e, "Exception thrown while handling options '{Type}' using the Async Execution Factory '{Factory}'.", result.TypeInfo.Current.Name, asyncExecutionFactory.GetType().Name);
                        }
            }

            // Throw exception if Parser ran but no service was found to handle the results.
            throw new NoExecuteCommandLineServiceFoundException(result.TypeInfo.Current, typeof(TResult), true,
                allowAsyncImplementations);
        }

        // ...Parser failed?

        // Look for Sync
        foreach (var syncExecutionFactory in _syncExecutionFactories)
            try
            {
                _log?.LogDebug("Handling parsing failure using the Sync Execution Factory '{Factory}'.", syncExecutionFactory.GetType().Name);
                var executionResult = syncExecutionFactory.ExecuteParsingFailure(args, errors);
                if (executionResult.Handled) return executionResult.Result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _log?.LogError(e, "Exception thrown while handling parsing failure using the Sync Execution Factory '{Factory}'.", syncExecutionFactory.GetType().Name);
            }

        // Look for Async
        if (allowAsyncImplementations)
            foreach (var asyncExecutionFactory in _asyncExecutionFactories)
                try
                {
                    _log?.LogDebug(
                        "Handling parsing failure using the Async Execution Factory '{Factory}'.",
                        asyncExecutionFactory.GetType().Name);
                    var executionResult = AsyncHelper.RunSync(async () => await asyncExecutionFactory.ExecuteParsingFailureAsync(args, errors, CancellationToken.None));
                    if (executionResult.Handled) return executionResult.Result;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _log?.LogError(e,
                        "Exception thrown while handling parsing failure using the Async Execution Factory '{Factory}'.", asyncExecutionFactory.GetType().Name);
                }

        // Return Default
        return defaultResult;
    }

    /// <summary>
    /// Parse Command Line Arguments using <see cref="Parser"/>.
    /// </summary>
    /// <param name="args">Command Line Arguments.</param>
    /// <param name="configuration">Optional Parser Configuration Action.</param>
    /// <param name="defaultResult">Default Result to return when parser was unable to parse out options.</param>
    /// <param name="allowSyncImplementations">Fall back to <see cref="IExecuteCommandLineOptions{TCommandLineOptions,TResult}"/> and/or <see cref="IExecuteParsingFailure{TResult}"/> implementations asynchronous version are not available?</param>
    /// <param name="ctx">Cancellation Token</param>
    /// <returns>Result [code].</returns>
    /// <exception cref="CommandLineOptionsValidationException">Options was found to be invalid (or there was an exception while validating the options)</exception>
    /// <exception cref="NoExecuteCommandLineServiceFoundException">No handler for Command line Options.</exception>
    public async Task<TResult?> ParseArgumentsAsync(string[] args, Action<ParserSettings>? configuration = null, TResult? defaultResult = default,
        bool allowSyncImplementations = true, CancellationToken ctx = default)
    {
        // Create Parser
        using var parser = configuration == null
            ? new Parser()
            : new Parser(configuration);

        // Execute Parser
        var result = _commandLineOptionTypes.Length == 1 && _commandLineOptionTypes.All(i => !i.GetCustomAttributes<VerbAttribute>().Any())
            ? parser.ParseArguments(() => Activator.CreateInstance(_commandLineOptionTypes.First()), args)
            : parser.ParseArguments(args, _commandLineOptionTypes);
        var errors = ((result as NotParsed<object>)?.Errors ?? []).ToArray();
        if (result.Tag == ParserResultType.Parsed)
            _log?.LogInformation("Command Line Arguments Parser determined that the '{Type}' type is to be used for options.", result.TypeInfo.Current.Name);
        else
            _log?.LogError("Command Line Arguments Parser was unable to parse arguments.");

        // Parser Execute successfully?
        if (result.Tag == ParserResultType.Parsed)
        {
            // Validate Options
            if (_commandLineOptionsValidator != null && result.Value is ICommandLineOptions clo && !await _commandLineOptionsValidator.ValidateAsync(clo, ctx))
                throw new CommandLineOptionsValidationException(clo);

            // Get Parsed Value
            if (result is Parsed<object> parsed)
            {
                // Look for Async Types to execute
                foreach (var asyncExecutionFactory in _asyncExecutionFactories)
                    try
                    {
                        _log?.LogDebug("Handling options '{Type}' using the Async Execution Factory '{Factory}'.", result.TypeInfo.Current.Name, asyncExecutionFactory.GetType().Name);
                        var executionResult = await asyncExecutionFactory.ExecuteCommandAsync(args, result.TypeInfo.Current, parsed.Value, ctx);
                        if (executionResult.Handled) return executionResult.Result;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        _log?.LogError(e, "Exception thrown while handling options '{Type}' using the Async Execution Factory '{Factory}'.", result.TypeInfo.Current.Name, asyncExecutionFactory.GetType().Name);
                    }

                // Look for Sync?
                if (allowSyncImplementations)
                    foreach (var syncExecutionFactory in _syncExecutionFactories)
                        try
                        {
                            _log?.LogDebug("Handling options '{Type}' using the Sync Execution Factory '{Factory}'.", result.TypeInfo.Current.Name, syncExecutionFactory.GetType().Name);
                            var executionResult =
                                syncExecutionFactory.ExecuteCommand(args, result.TypeInfo.Current, parsed.Value);
                            if (executionResult.Handled) return executionResult.Result;
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            _log?.LogError(e, "Exception thrown while handling options '{Type}' using the Sync Execution Factory '{Factory}'.", result.TypeInfo.Current.Name, syncExecutionFactory.GetType().Name);
                        }
            }

            // Throw exception if Parser ran but no service was found to handle the results.
            throw new NoExecuteCommandLineServiceFoundException(result.TypeInfo.Current, typeof(TResult), true,
                allowSyncImplementations);
        }

        // ...Parser failed?

        // Look for Async
        foreach (var asyncExecutionFactory in _asyncExecutionFactories)
            try
            {
                _log?.LogDebug(
                    "Handling parsing failure using the Async Execution Factory '{Factory}'.",
                    asyncExecutionFactory.GetType().Name);
                var executionResult = await asyncExecutionFactory.ExecuteParsingFailureAsync(args, errors, ctx);
                if (executionResult.Handled) return executionResult.Result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _log?.LogError(e,
                    "Exception thrown while handling parsing failure using the Async Execution Factory '{Factory}'.", asyncExecutionFactory.GetType().Name);
            }

        // Look for Sync
        if (allowSyncImplementations)
            foreach (var syncExecutionFactory in _syncExecutionFactories)
                try
                {
                    _log?.LogDebug("Handling parsing failure using the Sync Execution Factory '{Factory}'.", syncExecutionFactory.GetType().Name);
                    var executionResult = syncExecutionFactory.ExecuteParsingFailure(args, errors);
                    if (executionResult.Handled) return executionResult.Result;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _log?.LogError(e, "Exception thrown while handling parsing failure using the Sync Execution Factory '{Factory}'.", syncExecutionFactory.GetType().Name);
                }

        // Return Default
        return defaultResult;
    }

    #endregion
}