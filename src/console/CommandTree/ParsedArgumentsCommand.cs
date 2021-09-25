using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Shamir.Console
{
    public abstract class ParsedArgumentsCommand<TOptions> : ICommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public ImmutableArray<string> Arguments { get; private set; }

        public void Initialize(ReadOnlySpan<string> args)
        {
            Arguments = args.ToImmutableArray();
        }

        public async ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider)
        {
            using var parser = serviceProvider.GetRequiredService<Parser>();
            var result = parser.ParseArguments<TOptions>(Arguments);
            return await result.MapResult(
                options => ExecuteAsync(serviceProvider, options),
                errors =>
                {
                    var helpText = HelpText.AutoBuild(result);   
                    helpText.Heading = string.Empty;
                    helpText.Copyright = string.Empty;
                    System.Console.Error.WriteLine(helpText);
                    return ValueTask.FromResult(1);
                }
            );
        }

        public abstract ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider, TOptions options);
    }
}
