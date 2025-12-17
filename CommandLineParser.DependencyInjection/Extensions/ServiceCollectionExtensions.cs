using System;
using System.Linq;
using System.Reflection;
using CommandLineParser.DependencyInjection;
using CommandLineParser.DependencyInjection.Interfaces;
using CommandLineParser.DependencyInjection.Models;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Command Line Parser Extensions.
    /// </summary>
    /// <remarks>Only scans public classes.</remarks>
    /// <param name="services">Service Collection to add service to.</param>
    /// <param name="assemblies">Assemblies to scan.</param>
    public static IServiceCollection AddCommandLineParser(this IServiceCollection services, params Assembly[] assemblies) =>
        services.AddCommandLineParser(new CommandLineParserDiOptions(), assemblies);

    /// <summary>
    /// Add Command Line Parser Extensions.
    /// </summary>
    /// <remarks>Only scans public classes.</remarks>
    /// <param name="services">Service Collection to add service to.</param>
    /// <param name="configAction">Configure Options</param>
    /// <param name="assemblies">Assemblies to scan.</param>
    public static IServiceCollection AddCommandLineParser(this IServiceCollection services, Action<CommandLineParserDiOptions> configAction, params Assembly[] assemblies)
    {
        var options = new CommandLineParserDiOptions();
        configAction(options);
        return services.AddCommandLineParser(options, assemblies);
    }

    /// <summary>
    /// Add Command Line Parser Extensions.
    /// </summary>
    /// <param name="services">Service Collection to add service to.</param>
    /// <param name="options">Command Line Parser DI Options</param>
    /// <param name="assemblies">Assemblies to scan.</param>
    public static IServiceCollection AddCommandLineParser(this IServiceCollection services, CommandLineParserDiOptions? options = null, params Assembly[] assemblies)
    {
        var executeCommandLineOptionsInterface = typeof(IExecuteCommandLineOptions<,>);
        var executeParsingFailureInterface = typeof(IExecuteParsingFailure<>);
        var executeCommandLineOptionsAsyncInterface = typeof(IExecuteCommandLineOptionsAsync<,>);
        var executeParsingFailureAsyncInterface = typeof(IExecuteParsingFailureAsync<>);
        var commandLineParserSyncExecutionFactory = typeof(ICommandLineParserSyncExecutionFactory<>);
        var commandLineParserAsyncExecutionFactory = typeof(ICommandLineParserAsyncExecutionFactory<>);
        var defaultCommandLineParserAsyncExecutionFactory = typeof(CommandLineParserAsyncExecutionFactory<>);
        var defaultCommandLineParserSyncExecutionFactory =  typeof(CommandLineParserSyncExecutionFactory<>);
        var validateCommandLineOptions = typeof(IValidateCommandLineOptions<>);
        var validateCommandLineOptionsAsync = typeof(IValidateCommandLineOptionsAsync<>);

        // Default Options
        options ??= new CommandLineParserDiOptions();

        // Include Calling assembly?
        if (options.AlwaysIncludeCallingAssembly)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            if (!assemblies.Contains(callingAssembly))
            {
                var assembliesList = assemblies.ToList();
                assembliesList.Add(callingAssembly);
                assemblies = assembliesList.ToArray();
            }
        }

        // Command Line Options
        services
                .Scan(a => a
                    .FromAssemblies(assemblies)
                    .AddClasses(i => i.AssignableTo<ICommandLineOptions>(), publicOnly: options.PublicOnly)
                    .As<ICommandLineOptions>()
                    .WithTransientLifetime()
                );

        // Async Services
        if (options.FindAsyncServices)
            services
                .Scan(a => a
                    .FromAssemblies(assemblies)
                    .AddClasses(i => i
                        .AssignableTo(executeCommandLineOptionsAsyncInterface)
                        .Where(t => !t.IsAbstract)
                        , publicOnly: options.PublicOnly)
                    .As(t => t.GetInterfaces().Where(i => i.IsConstructedGenericType && executeCommandLineOptionsAsyncInterface.IsAssignableFrom(i.GetGenericTypeDefinition())))
                    .WithTransientLifetime()
                )
                .Scan(a => a
                    .FromAssemblies(assemblies)
                    .AddClasses(i => i
                            .AssignableTo(executeParsingFailureAsyncInterface)
                        .Where(t => !t.IsAbstract)
                        , publicOnly: options.PublicOnly)
                    .As(t => t.GetInterfaces().Where(i => i.IsConstructedGenericType && executeParsingFailureAsyncInterface.IsAssignableFrom(i.GetGenericTypeDefinition())))
                    .WithTransientLifetime()
                )
                .Scan(a => a
                    .FromAssemblies(assemblies)
                    .AddClasses(i => i
                        .AssignableTo(commandLineParserAsyncExecutionFactory)
                        .Where(t => !t.IsAbstract && t != defaultCommandLineParserAsyncExecutionFactory)
                        , publicOnly: options.PublicOnly)
                    .As(t => t.GetInterfaces().Where(i => i.IsConstructedGenericType && commandLineParserAsyncExecutionFactory.IsAssignableFrom(i.GetGenericTypeDefinition())))
                    .WithSingletonLifetime()
                )
                .Scan(a => a
                    .FromAssemblies(assemblies)
                    .AddClasses(i => i
                            .AssignableTo(validateCommandLineOptionsAsync)
                            .Where(t => !t.IsAbstract)
                        , publicOnly: options.PublicOnly)
                    .As(t => t.GetInterfaces().Where(i => i.IsConstructedGenericType && validateCommandLineOptionsAsync.IsAssignableFrom(i.GetGenericTypeDefinition())))
                    .WithTransientLifetime()
                )
                ;
        if (options.IncludeDefaultAsyncFactory)
            services
                .AddSingleton(typeof(ICommandLineParserAsyncExecutionFactory<>), typeof(CommandLineParserAsyncExecutionFactory<>))
                ;

        // Sync Services
        if (options.FindSyncServices)
            services
                .Scan(a => a
                    .FromAssemblies(assemblies)
                    .AddClasses(i => i
                        .AssignableTo(executeCommandLineOptionsInterface)
                        .Where(t => !t.IsAbstract)
                        , publicOnly: options.PublicOnly)
                    .As(t => t.GetInterfaces().Where(i => i.IsConstructedGenericType && executeCommandLineOptionsInterface.IsAssignableFrom(i.GetGenericTypeDefinition())))
                    .WithTransientLifetime()
                )
                .Scan(a => a
                    .FromAssemblies(assemblies)
                    .AddClasses(i => i
                        .AssignableTo(executeParsingFailureInterface)
                        .Where(t => !t.IsAbstract)
                        , publicOnly: options.PublicOnly)
                    .As(t => t.GetInterfaces().Where(i => i.IsConstructedGenericType && executeParsingFailureInterface.IsAssignableFrom(i.GetGenericTypeDefinition())))
                    .WithTransientLifetime()
                )
                .Scan(a => a
                    .FromAssemblies(assemblies)
                    .AddClasses(i => i
                        .AssignableTo(commandLineParserSyncExecutionFactory)
                        .Where(t => !t.IsAbstract && t != defaultCommandLineParserSyncExecutionFactory)
                        , publicOnly: options.PublicOnly)
                    .As(t => t.GetInterfaces().Where(i => i.IsConstructedGenericType && commandLineParserSyncExecutionFactory.IsAssignableFrom(i.GetGenericTypeDefinition())))
                    .WithSingletonLifetime()
                )
                .Scan(a => a
                    .FromAssemblies(assemblies)
                    .AddClasses(i => i
                            .AssignableTo(validateCommandLineOptions)
                            .Where(t => !t.IsAbstract)
                        , publicOnly: options.PublicOnly)
                    .As(t => t.GetInterfaces().Where(i => i.IsConstructedGenericType && validateCommandLineOptions.IsAssignableFrom(i.GetGenericTypeDefinition())))
                    .WithTransientLifetime()
                )
                ;
        if (options.IncludeDefaultSyncFactory)
            services
                .AddSingleton(typeof(ICommandLineParserSyncExecutionFactory<>), typeof(CommandLineParserSyncExecutionFactory<>))
                ;

        // Validator
        if (options.IncludeDefaultOptionsValidator)
            services
                .AddSingleton<ICommandLineOptionsValidator, CommandLineOptionsValidator>()
                ;

        // Core Services
        services
            .AddSingleton(typeof(ICommandLineParser<>), typeof(CommandLineParser<>))
            ;

        return services;
    }
}
