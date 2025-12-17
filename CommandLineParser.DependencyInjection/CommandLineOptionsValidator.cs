using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandLineParser.DependencyInjection.Exceptions;
using CommandLineParser.DependencyInjection.Extensions;
using CommandLineParser.DependencyInjection.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommandLineParser.DependencyInjection;

public class CommandLineOptionsValidator(
    IServiceProvider serviceProvider,
    ILogger<CommandLineOptionsValidator>? log = null
) : ICommandLineOptionsValidator
{
    private static readonly Type SyncValidatorType = typeof(IValidateCommandLineOptions<>);
    private static readonly Type AsyncValidatorType = typeof(IValidateCommandLineOptionsAsync<>);

    /// <summary>
    /// Options Type to Service Type Map
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<Type, (Type ServiceType, MethodInfo ValidateMethodInfo)> OptionTypeToSyncValidatorType = new();
    private static readonly ConcurrentDictionary<Type, (Type ServiceType, MethodInfo ValidateMethodInfo)> OptionTypeToAsyncValidatorType = new();

    /// <summary>
    /// Try to Validate Options Synchronously.
    /// </summary>
    /// <param name="options">Options to Validate</param>
    /// <param name="type">Options Type</param>
    /// <returns>If there was a service available and if it was considered valid or not.</returns>
    private (bool Available, bool Valid) ValidateUsingSyncValidator(ICommandLineOptions options, Type type)
    {
        var serviceInfo = OptionTypeToSyncValidatorType.GetOrAdd(type, t =>
        {
            var validatorType = SyncValidatorType.MakeGenericType(t);
            var methodInfo = validatorType.GetMethod("Validate");
            return (ServiceType: validatorType, ValidateMethodInfo: methodInfo);
        });
        var validatorService = serviceProvider.GetService(serviceInfo.ServiceType);
        if (validatorService == null)
            return (false, false);
        var isValid = (bool)serviceInfo.ValidateMethodInfo.Invoke(validatorService, [options])!;
        return (true, isValid);
    }

    /// <summary>
    /// Try to Validate Options Synchronously.
    /// </summary>
    /// <param name="options">Options to Validate</param>
    /// <param name="type">Options Type</param>
    /// <param name="ctx">Cancellation Token</param>
    /// <returns>If there was a service available and if it was considered valid or not.</returns>
    private async Task<(bool Available, bool Valid)> ValidateUsingAsyncValidatorAsync(ICommandLineOptions options, Type type, CancellationToken ctx)
    {
        var serviceInfo = OptionTypeToAsyncValidatorType.GetOrAdd(type, t =>
        {
            var validatorType = AsyncValidatorType.MakeGenericType(t);
            var methodInfo = validatorType.GetMethod("ValidateAsync");
            return (ServiceType: validatorType, ValidateMethodInfo: methodInfo);
        });
        var validatorService = serviceProvider.GetService(serviceInfo.ServiceType);
        if (validatorService == null)
            return (false, false);
        var isValid = await serviceInfo.ValidateMethodInfo.InvokeAsync<bool>(validatorService, [options, ctx]);
        return (true, isValid);
    }

    #region Implementation of ICommandLineOptionsValidator

    /// <summary>
    /// Validate Options Synchronously.
    /// </summary>
    /// <remarks>Prefers Synchronous validators but looks for Asynchronous validators as a fallback.</remarks>
    /// <param name="options">Options to validate.</param>
    /// <returns>Validation Result</returns>
    /// <exception cref="CommandLineOptionsValidationException">Thrown when the validator has an exception.</exception>
    public bool Validate(ICommandLineOptions options)
    {
        var type = options.GetType();

        try
        {
            // Try to find Synchronous Validators
            var validationResults = ValidateUsingSyncValidator(options, type);
            if (validationResults.Available)
            {
                log?.LogDebug($"Validated Options '{type.Name}' with Sync validator.  Options are {(validationResults.Valid ? "valid" : "not valid")}.");
                return validationResults.Valid;
            }

            // Try to find Asynchronous Validators
            var asyncValidationResults = AsyncHelper.RunSync(async () => await ValidateUsingAsyncValidatorAsync(options, type, CancellationToken.None));
            if (asyncValidationResults.Available)
            {
                log?.LogDebug($"Validated Options '{type.Name}' with Async validator.  Options are {(asyncValidationResults.Valid ? "valid" : "not valid")}.");
                return asyncValidationResults.Valid;
            }

            // No Validators found
            log?.LogDebug($"No Validator found for Options '{type.Name}'.");
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            var ex = new CommandLineOptionsValidationException(options, e);
            log?.LogError(ex, $"Unable to validate options '{type.Name}'.");
            throw ex;
        }
    }

    /// <summary>
    /// Validate Options Asynchronously.
    /// </summary>
    /// <remarks>Prefers Asynchronous validators but looks for Synchronous validators as a fallback.</remarks>
    /// <param name="options">Options to validate.</param>
    /// <param name="ctx">Cancellation Token</param>
    /// <returns>Validation Result</returns>
    /// <exception cref="CommandLineOptionsValidationException">Thrown when the validator has an exception.</exception>
    public async Task<bool> ValidateAsync(ICommandLineOptions options, CancellationToken ctx)
    {
        var type = options.GetType();

        try
        {
            // Try to find Asynchronous Validators
            var asyncValidationResults = await ValidateUsingAsyncValidatorAsync(options, type, ctx);
            if (asyncValidationResults.Available)
            {
                log?.LogDebug($"Validated Options '{type.Name}' with Async validator.  Options are {(asyncValidationResults.Valid ? "valid" : "not valid")}.");
                return asyncValidationResults.Valid;
            }

            // Try to find Synchronous Validators
            var validationResults = ValidateUsingSyncValidator(options, type);
            if (validationResults.Available)
            {
                log?.LogDebug($"Validated Options '{type.Name}' with Sync validator.  Options are {(validationResults.Valid ? "valid" : "not valid")}.");
                return validationResults.Valid;
            }

            // No Validators found
            log?.LogDebug($"No Validator found for Options '{type.Name}'.");
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            var ex = new CommandLineOptionsValidationException(options, e);
            log?.LogError(ex, $"Unable to validate options '{type.Name}'.");
            throw ex;
        }
    }

    #endregion
}