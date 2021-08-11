using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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
            Assert.Equal("I do not like them, Sam I Am! I do not like ASYNC Green Eggs and Ham.", service.ParseArguments(new[] { "askAsync", "Green Eggs and Ham" }));
            Assert.Equal("Yes, I do like ASYNC Green Eggs and Ham! Thank you, Thank you Sam I Am!", service.ParseArguments(new[] { "askAsync", "Green Eggs and Ham", "--like", "true" }));
        }

        [Fact]
        public async Task AskOptionsExecutionAsyncTest()
        {
            var service = ServiceProvider.GetRequiredService<ICommandLineParser<string>>();
            Assert.Equal("I do not like them, Sam I Am! I do not like Green Eggs and Ham.", await service.ParseArgumentsAsync(new[] { "ask", "Green Eggs and Ham" }));
            Assert.Equal("Yes, I do like Green Eggs and Ham! Thank you, Thank you Sam I Am!", await service.ParseArgumentsAsync(new[] { "ask", "Green Eggs and Ham", "--like", "true" }));
            Assert.Equal("I do not like them, Sam I Am! I do not like ASYNC Green Eggs and Ham.", await service.ParseArgumentsAsync(new[] { "askAsync", "Green Eggs and Ham" }));
            Assert.Equal("Yes, I do like ASYNC Green Eggs and Ham! Thank you, Thank you Sam I Am!", await service.ParseArgumentsAsync(new[] { "askAsync", "Green Eggs and Ham", "--like", "true" }));
        }

        [Fact]
        public void OptionsExecutionFailureTest()
        {
            var service = ServiceProvider.GetRequiredService<ICommandLineParser<string>>();
            Assert.Equal("Unable to parse \"-filename testfile.txt\".", service.ParseArguments(new[] { "-filename", "testfile.txt" }));
        }

        [Fact]
        public async Task OptionsExecutionFailureAsyncTest()
        {
            var service = ServiceProvider.GetRequiredService<ICommandLineParser<string>>();
            Assert.Equal("Unable to parse \"-filename testfile.txt\" ASYNC.", await service.ParseArgumentsAsync(new[] { "-filename", "testfile.txt" }));
        }

        [Fact]
        public void HelpTests()
        {
            var name = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? Assembly.GetCallingAssembly().GetName().Name;
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? Assembly.GetCallingAssembly().GetName().Version.ToString();
            var service = ServiceProvider.GetRequiredService<ICommandLineParser<string>>();
            using (var writer = new StringWriter())
            {
                service.ParseArguments(new string[0], o => o.HelpWriter = writer);
                Assert.Equal($"{name} {version}\r\nCopyright (C) 2021 JetBrains s.r.o.\r\n\r\nERROR(S):\r\n  No verb selected.\r\n\r\n  ask         Ask a question.\r\n\r\n  askAsync    Ask a question ASYNC.\r\n\r\n  help        Display more information on a specific command.\r\n\r\n  version     Display version information.\r\n\r\n", writer.ToString());
            }
            using (var writer = new StringWriter())
            {
                service.ParseArguments(new [] {"ask", "--help"}, o => o.HelpWriter = writer);
                Assert.Equal($"{name} {version}\r\nCopyright (C) 2021 JetBrains s.r.o.\r\nUSAGE:\r\nDo you like green eggs and ham?:\r\n  CommandLineParserDiTests ask --like \"Green Eggs and Ham?\"\r\n\r\n  --like          (Default: false) Should we like this?\r\n\r\n  --help          Display this help screen.\r\n\r\n  --version       Display version information.\r\n\r\n  value pos. 0    Required. What do we like?\r\n\r\n", writer.ToString());
            }
        }
    }
}
