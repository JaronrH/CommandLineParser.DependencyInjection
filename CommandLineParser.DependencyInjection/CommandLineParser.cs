using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLineParser.DependencyInjection.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CommandLineParser.DependencyInjection
{
    public class CommandLineParser<TResult> : ICommandLineParser<TResult>
    {
        private static readonly Type ExecuteCommandLineOptionsInterfaceType = typeof(IExecuteCommandLineOptions<,>);
        private readonly IEnumerable<ICommandLineOptions> _commandLineOptions;
        private readonly IServiceProvider _serviceProvider;

        public CommandLineParser(IEnumerable<ICommandLineOptions> commandLineOptions, IServiceProvider serviceProvider)
        {
            _commandLineOptions = commandLineOptions;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Parse Command Line Arguments using <see cref="Parser"/>.
        /// </summary>
        /// <param name="args">Command Line Arguments.</param>
        /// <param name="configuration">Optional Parser Configuration Action.</param>
        /// <returns>Result [code].</returns>
        public TResult ParseArguments(string[] args, Action<ParserSettings> configuration)
        {
            var parser = configuration == null
                ? new Parser()
                : new Parser(configuration);
            var result = parser.ParseArguments(args, _commandLineOptions.Select(i => i.GetType()).ToArray());
            if (result.Tag == ParserResultType.Parsed)
            {
                var type =
                    ExecuteCommandLineOptionsInterfaceType.MakeGenericType(result.TypeInfo.Current, typeof(TResult));
                var methodType = type.GetMethod("Execute");
                var executingService = _serviceProvider.GetRequiredService(type);
                if (result is Parsed<object> parsed)
                    return (TResult) methodType.Invoke(executingService, new[] {parsed.Value});
            }
            var service = _serviceProvider.GetService<IExecuteParsingFailure<TResult>>();
            return service == null
                ? default(TResult)
                : service.Execute(args, (result as NotParsed<object>)?.Errors ?? Enumerable.Empty<Error>());
        }
    }
}