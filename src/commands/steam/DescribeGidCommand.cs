using System;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Shamir.Abstractions;
using SteamKit2;

namespace Shamir.Commands.Radio
{
    public class DescribeGidOptions
    {
        [Value(0, MetaName = "gid", Required = true, HelpText = "Global ID to describe.")]
        public ulong Gid { get; set; }
    }

    public sealed class DescribeGidCommand : ParsedArgumentsCommand<DescribeGidOptions>
    {
        public override string Name => "gid";

        public override string Description => "Explain a gid_t (Global ID)";

        public override ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider, DescribeGidOptions options)
        {
            var gid = new GlobalID(options.Gid);

            var console = serviceProvider.GetRequiredService<IConsole>();

            console.Output.WriteLine($"Box ID             : {gid.BoxID}");
            console.Output.WriteLine($"Process ID         : {gid.ProcessID}");
            console.Output.WriteLine($"Process Start Time : {gid.StartTime}");
            console.Output.WriteLine($"Sequence           : {gid.SequentialCount}");

            return ValueTask.FromResult(0);
        }
    }
}