using System;
using System.Threading.Tasks;
using CommandLine;
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

            Console.WriteLine($"Box ID             : {gid.BoxID}");
            Console.WriteLine($"Process ID         : {gid.ProcessID}");
            Console.WriteLine($"Process Start Time : {gid.StartTime}");
            Console.WriteLine($"Sequence           : {gid.SequentialCount}");

            return ValueTask.FromResult(0);
        }
    }
}