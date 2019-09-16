using System;
using CommandLineParser.DependencyInjection.Interfaces;
using CommandLineParser.DependencyInjection.Tests.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommandLineParser.DependencyInjection.Tests
{
    public class CommandLineParserDiTests
    {
        protected IServiceProvider ServiceProvider { get; set; }

        public CommandLineParserDiTests()
        {
            var collection = new ServiceCollection()
                .AddCommandLineParser(typeof(CommandLineParserDiTests).Assembly)
                .AddSingleton<DoYouLikeService>()
                ;
            ServiceProvider = collection.BuildServiceProvider();
        }

        [Fact]
        public void AskOptionsExecutionTest()
        {
            var service = ServiceProvider.GetRequiredService<ICommandLineParser<string>>();
            Assert.Equal("I do not like them, Sam I Am! I do not like Green Eggs and Ham.", service.ParseArguments(new[] { "ask", "Green Eggs and Ham" }));
            Assert.Equal("Yes, I do like Green Eggs and Ham! Thank you, Thank you Sam I Am!", service.ParseArguments(new[] { "ask", "Green Eggs and Ham", "--like", "true" }));
        }

        [Fact]
        public void OptionsExecutionFailureTest()
        {
            var service = ServiceProvider.GetRequiredService<ICommandLineParser<string>>();
            Assert.Equal("Unable to parse \"-filename testfile.txt\".", service.ParseArguments(new[] { "-filename", "testfile.txt" }));
        }
    }
}
