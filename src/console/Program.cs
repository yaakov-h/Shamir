using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Shamir.Console
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var tree = new DefaultCommandTree(
                "shamir",
                "command-line multitool",
                ImmutableArray.Create<ICommandTree>(

                ),
                ImmutableArray.Create<ICommand>(

                )
            );

            var command = tree.FindCommand(args);
            return await command.ExecuteAsync();
        }
    }
}
