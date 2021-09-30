using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shamir.Abstractions;
using Shamir.Commands.Azure;
using Shamir.Commands.Radio;

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
                .AddSingleton<IConsole, SystemConsole>()
                .AddAzureCommandTree()
                .AddRadioCommandTree()
                .AddSteamCommandTree()
                .BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();

            var tree = new DefaultCommandTree(
                "shamir",
                "command-line multitool",
                scope.ServiceProvider.GetServices<ICommandTree>().OrderBy(x => x.Name).ToImmutableArray(),
                scope.ServiceProvider.GetServices<ICommand>().OrderBy(x => x.Name).ToImmutableArray()
            );

            var command = tree.FindCommand(args);

            return await command.ExecuteAsync(scope.ServiceProvider);
        }
    }
}
