using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using CommandLineParser.DependencyInjection.Exceptions;
using CommandLineParser.DependencyInjection.Extensions;
using CommandLineParser.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CommandLineParser.DependencyInjection
{
    public class CommandLineParser<TResult> : ICommandLineParser<TResult>
    {
        private static readonly Type ExecuteCommandLineOptionsInterfaceType = typeof(IExecuteCommandLineOptions<,>);
        private static readonly Type ExecuteCommandLineOptionsAsyncInterfaceType = typeof(IExecuteCommandLineOptionsAsync<,>);
        private readonly Type[] _commandLineOptionTypes;
        private readonly IServiceProvider _serviceProvider;

        public CommandLineParser(IEnumerable<ICommandLineOptions> commandLineOptions, IServiceProvider serviceProvider)
        {
            _commandLineOptionTypes = commandLineOptions.Select(i => i.GetType()).ToArray();
            _serviceProvider = serviceProvider;
        }

        #region Implementation of ICommandLineParser<TResult>

        /// <summary>
        /// Parse Command Line Arguments using <see cref="Parser"/>.
        /// </summary>
        /// <param name="args">Command Line Arguments.</param>
        /// <param name="configuration">Optional Parser Configuration Action.</param>
        /// <param name="defaultResult">Default Result to return when parser was unable to parse out options.</param>
        /// <param name="allowAsyncImplementations">Fall back to <see cref="IExecuteCommandLineOptionsAsync{TCommandLineOptions,TResult}"/> and/or <see cref="IExecuteParsingFailureAsync{TResult}"/> implementations an run them synchronously when synchronous version are not available?</param>
        /// <returns>Result [code].</returns>
        public TResult ParseArguments(string[] args, Action<ParserSettings> configuration = null, TResult defaultResult = default,
            bool allowAsyncImplementations = true)
        {
            // Create Parser
            using var parser = configuration == null
                ? new Parser()
                : new Parser(configuration);

            // Execute Parser
            var result = _commandLineOptionTypes.Count() == 1 && _commandLineOptionTypes.All(i => !i.GetCustomAttributes<VerbAttribute>().Any())
                ? parser.ParseArguments(() => Activator.CreateInstance(_commandLineOptionTypes.First()), args)
                : parser.ParseArguments(args, _commandLineOptionTypes);

            // Parser Execute successfully?
            if (result.Tag == ParserResultType.Parsed)
            {
                // Get Parsed Value
                var parsed = result as Parsed<object>;

                // Look for Sync Types to execute
                var type =
                    ExecuteCommandLineOptionsInterfaceType.MakeGenericType(result.TypeInfo.Current,
                        typeof(TResult));
                var methodType = type.GetMethod("Execute");
                var executingService = _serviceProvider.GetService(type);
                if (executingService != null && parsed != null)
                    return (TResult)methodType.Invoke(executingService, new[] { parsed.Value });

                // Look for Async?
                if (allowAsyncImplementations)
                {
                    type =
                        ExecuteCommandLineOptionsAsyncInterfaceType.MakeGenericType(result.TypeInfo.Current,
                            typeof(TResult));
                    methodType = type.GetMethod("ExecuteAsync");
                    executingService = _serviceProvider.GetService(type);
                    if (executingService != null && parsed != null)
                        return AsyncHelper.RunSync(async () => (TResult)(await methodType.InvokeAsync(executingService, new[] { parsed.Value })));
                }

                // Throw exception if Parser ran but no service was found to handle the results.
                throw new NoExecuteCommandLineServiceFoundException(result.TypeInfo.Current, typeof(TResult), true,
                    allowAsyncImplementations);
            }

            // ...Parser failed?
            var service = _serviceProvider.GetService<IExecuteParsingFailure<TResult>>();
            var serviceAsync = _serviceProvider.GetService<IExecuteParsingFailureAsync<TResult>>();
            return service == null
                ? serviceAsync == null || !allowAsyncImplementations
                    ? defaultResult
                    : AsyncHelper.RunSync(async () => await serviceAsync.ExecuteAsync(args, (result as NotParsed<object>)?.Errors ?? Enumerable.Empty<Error>()))
                : service.Execute(args, (result as NotParsed<object>)?.Errors ?? Enumerable.Empty<Error>());
        }

        /// <summary>
        /// Parse Command Line Arguments using <see cref="Parser"/>.
        /// </summary>
        /// <param name="args">Command Line Arguments.</param>
        /// <param name="configuration">Optional Parser Configuration Action.</param>
        /// <param name="defaultResult">Default Result to return when parser was unable to parse out options.</param>
        /// <param name="allowSyncImplementations">Fall back to <see cref="IExecuteCommandLineOptions{TCommandLineOptions,TResult}"/> and/or <see cref="IExecuteParsingFailure{TResult}"/> implementations asynchronous version are not available?</param>
        /// <returns>Result [code].</returns>
        public async Task<TResult> ParseArgumentsAsync(string[] args, Action<ParserSettings> configuration = null, TResult defaultResult = default,
            bool allowSyncImplementations = true)
        {
            // Create Parser
            using var parser = configuration == null
                ? new Parser()
                : new Parser(configuration);

            // Execute Parser
            var result = _commandLineOptionTypes.Count() == 1 && _commandLineOptionTypes.All(i => !i.GetCustomAttributes<VerbAttribute>().Any())
                ? parser.ParseArguments(() => Activator.CreateInstance(_commandLineOptionTypes.First()), args)
                : parser.ParseArguments(args, _commandLineOptionTypes);

            // Parser Execute successfully?
            if (result.Tag == ParserResultType.Parsed)
            {
                // Get Parsed Value
                var parsed = result as Parsed<object>;

                // Look for Sync Types to execute
                var type =
                    ExecuteCommandLineOptionsAsyncInterfaceType.MakeGenericType(result.TypeInfo.Current,
                        typeof(TResult));
                var methodType = type.GetMethod("ExecuteAsync");
                var executingService = _serviceProvider.GetService(type);
                if (executingService != null && parsed != null)
                    return (TResult)(await methodType.InvokeAsync(executingService, new[] { parsed.Value }));

                // Look for Async?
                if (allowSyncImplementations)
                {
                    type =
                        ExecuteCommandLineOptionsInterfaceType.MakeGenericType(result.TypeInfo.Current,
                            typeof(TResult));
                    methodType = type.GetMethod("Execute");
                    executingService = _serviceProvider.GetService(type);
                    if (executingService != null && parsed != null)
                        return (TResult)methodType.Invoke(executingService, new[] { parsed.Value });
                }

                // Throw exception if Parser ran but no service was found to handle the results.
                throw new NoExecuteCommandLineServiceFoundException(result.TypeInfo.Current, typeof(TResult), true,
                    allowSyncImplementations);
            }

            // ...Parser failed?
            var serviceAsync = _serviceProvider.GetService<IExecuteParsingFailureAsync<TResult>>();
            var service = _serviceProvider.GetService<IExecuteParsingFailure<TResult>>();
            return serviceAsync == null
                ? service == null || !allowSyncImplementations
                    ? defaultResult
                    : service.Execute(args, (result as NotParsed<object>)?.Errors ?? Enumerable.Empty<Error>())
                : await serviceAsync.ExecuteAsync(args, (result as NotParsed<object>)?.Errors ?? Enumerable.Empty<Error>());
        }

        #endregion
    }
}