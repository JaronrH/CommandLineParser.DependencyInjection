using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLineParser.DependencyInjection.Interfaces;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Command Line Parser Extensions.
        /// </summary>
        /// <param name="services">Service Collection to add service to.</param>
        /// <param name="assemblies">Assemblies to scan for <see cref="ICommandLineOptions"/>, <see cref="IExecuteCommandLineOptions{TCommandLineOptions,TResult}"/>, and <see cref="IExecuteParsingFailure{TResult}"/>.</param>
        public static IServiceCollection AddCommandLineParser(this IServiceCollection services,
            params Assembly[] assemblies)
        {
            var executeCommandLineOptionsInterface = typeof(IExecuteCommandLineOptions<,>);
            var executeParsingFailureInterface = typeof(IExecuteParsingFailure<>);
            return services
                    .Scan(a => a
                        .FromAssemblies(assemblies)
                        .AddClasses(i => i.AssignableTo<ICommandLineOptions>())
                        .As<ICommandLineOptions>()
                    )
                    .Scan(a => a
                        .FromAssemblies(assemblies)
                        .AddClasses(i => i.AssignableTo(executeCommandLineOptionsInterface))
                        .As(t => t.GetInterfaces().Where(i => i.IsConstructedGenericType && executeCommandLineOptionsInterface.IsAssignableFrom(i.GetGenericTypeDefinition())))
                    )
                    .Scan(a => a
                        .FromAssemblies(assemblies)
                        .AddClasses(i => i.AssignableTo(executeParsingFailureInterface))
                        .As(t => t.GetInterfaces().Where(i => i.IsConstructedGenericType && executeParsingFailureInterface.IsAssignableFrom(i.GetGenericTypeDefinition())))
                    )
                    .AddSingleton(typeof(ICommandLineParser<>), typeof(CommandLineParser.DependencyInjection.CommandLineParser<>))
                ;
            ;
        }
    }
}
