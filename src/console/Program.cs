using System.Collections.Immutable;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Shamir.Console
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            await using var serviceProvider = new ServiceCollection()
                .AddTransient<Parser>(sp => new Parser(with =>
                {
                    with.AutoHelp = true;
                    with.AutoVersion = true;
                    with.EnableDashDash = true;
                    with.IgnoreUnknownArguments = false;
                }))
                .BuildServiceProvider();

            var tree = new DefaultCommandTree(
                "shamir",
                "command-line multitool",
                ImmutableArray.Create<ICommandTree>(
                    new DefaultCommandTree(
                        "cdn",
                        "Browse and add new data to a CDN backed by Azure Storage",
                        ImmutableArray<ICommandTree>.Empty,
                        ImmutableArray.Create<ICommand>(
                            new StorageLsCommand(),
                            new StorageCopyCommand(),
                            new StorageGetUrlCommand()
                        ))
                ),
                ImmutableArray<ICommand>.Empty
            );

            var command = tree.FindCommand(args);

            using var scope = serviceProvider.CreateScope();
            return await command.ExecuteAsync(scope.ServiceProvider);
        }
    }
}
