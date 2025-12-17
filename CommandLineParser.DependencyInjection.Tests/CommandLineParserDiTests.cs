using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLineParser.DependencyInjection.Interfaces;
using CommandLineParser.DependencyInjection.Tests.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommandLineParser.DependencyInjection.Tests;

public class CommandLineParserDiTests
{
    protected IServiceProvider ServiceProvider { get; set; }

    public CommandLineParserDiTests()
    {
        var collection = new ServiceCollection()
            .AddCommandLineParser(i => i.PublicOnly = false, typeof(CommandLineParserDiTests).Assembly)
            .AddSingleton<DoYouLikeService>()
            ;
        ServiceProvider = collection.BuildServiceProvider();
    }

    public static IEnumerable<object[]> AskOptionsTestData =>
        new List<object[]>
        {
            new object[] { (string[])["ask", "Green Eggs and Ham"], "I do not like them, Sam I Am! I do not like Green Eggs and Ham." },
            new object[] { (string[])["ask", "Green Eggs and Ham", "--like", "true"], "Yes, I do like Green Eggs and Ham! Thank you, Thank you Sam I Am!" },
            new object[] { (string[])["askAsync", "Green Eggs and Ham"], "I do not like them, Sam I Am! I do not like ASYNC Green Eggs and Ham." },
            new object[] { (string[])["askAsync", "Green Eggs and Ham", "--like", "true"], "Yes, I do like ASYNC Green Eggs and Ham! Thank you, Thank you Sam I Am!" },
        };

    public static IEnumerable<object[]> AskOptionsSyncTestData => AskOptionsTestData.Concat(
        new List<object[]>
        {
            new object[] { (string[])["-filename", "testfile.txt"], "Unable to parse \"-filename testfile.txt\"." },
        });

    public static IEnumerable<object[]> AskOptionsAsyncTestData => AskOptionsTestData.Concat(
        new List<object[]>
        {
            new object[] { (string[])["-filename", "testfile.txt"], "Unable to parse \"-filename testfile.txt\" ASYNC." },
        });

    [Theory]
    [MemberData(nameof(AskOptionsSyncTestData))]
    public void AskOptionsExecutionTest(string[] arguments, string expectedResult)
    {
        var service = ServiceProvider.GetRequiredService<ICommandLineParser<string>>();
        service
            .ParseArguments(arguments)
            .Should()
            .Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(AskOptionsAsyncTestData))]
    public async Task AskOptionsExecutionAsyncTest(string[] arguments, string expectedResult)
    {
        var service = ServiceProvider.GetRequiredService<ICommandLineParser<string>>();
        (await service
            .ParseArgumentsAsync(arguments))
            .Should()
            .Be(expectedResult);
    }

    [Fact]
    public void HelpTests()
    {
        var name = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? Assembly.GetCallingAssembly().GetName().Name;
        var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? Assembly.GetCallingAssembly().GetName().Version.ToString();
        var service = ServiceProvider.GetRequiredService<ICommandLineParser<string>>();
        using (var writer = new StringWriter())
        {
            service.ParseArguments([], o => o.HelpWriter = writer);
            writer
                .ToString()
                .Should()
                .Be($"{name} {version}\r\nCopyright (C) 2025 JetBrains s.r.o.\r\n\r\nERROR(S):\r\n  No verb selected.\r\n\r\n  ask         Ask a question.\r\n\r\n  askAsync    Ask a question ASYNC.\r\n\r\n  help        Display more information on a specific command.\r\n\r\n  version     Display version information.\r\n\r\n");
        }
        using (var writer = new StringWriter())
        {
            service.ParseArguments(["ask", "--help"], o => o.HelpWriter = writer);
            writer
                .ToString()
                .Should()
                .Be($"{name} {version}\r\nCopyright (C) 2025 JetBrains s.r.o.\r\nUSAGE:\r\nDo you like green eggs and ham?:\r\n  CommandLineParserDiTests ask --like \"Green Eggs and Ham?\"\r\n\r\n  --like          (Default: false) Should we like this?\r\n\r\n  --help          Display this help screen.\r\n\r\n  --version       Display version information.\r\n\r\n  value pos. 0    Required. What do we like?\r\n\r\n");
        }
    }
}