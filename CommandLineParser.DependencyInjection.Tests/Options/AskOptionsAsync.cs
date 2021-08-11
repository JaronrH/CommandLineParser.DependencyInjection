using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using CommandLineParser.DependencyInjection.Interfaces;

namespace CommandLineParser.DependencyInjection.Tests.Options
{
    [Verb("askAsync", HelpText = "Ask a question ASYNC.")]
    class AskOptionsAsync : ICommandLineOptions
    {
        [Option("like", Required = false, Default = false, HelpText = "Should we like this?")]
        public bool Like { get; set; }

        [Value(0, Required = true, HelpText = "What do we like?")]
        public string DoYouLike { get; set; }

        [Usage(ApplicationAlias = "CommandLineParserDiTests")]
        public static IEnumerable<Example> Examples =>
            new List<Example>() {
                new Example("Do you like green eggs and ham?", new AskOptions { DoYouLike = "Green Eggs and Ham?", Like = true })
            };
    }
}