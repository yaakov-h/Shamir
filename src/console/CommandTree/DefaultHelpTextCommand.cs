using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Shamir.Console
{
    public sealed class DefaultHelpTextCommand : ICommand
    {
        public DefaultHelpTextCommand(IImmutableStack<ICommandTree> path)
        {
            this.Path = path;
        }

        public string Name => "help";
        public string Description => "Print help text";

        public IImmutableStack<ICommandTree> Path { get; }

        public void Initialize(ReadOnlySpan<string> args)
        {
        }
        
        public ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider)
        {
            System.Console.Error.WriteLine(GetHelpText());
            return ValueTask.FromResult(1); // TODO: const
        }

        public string GetHelpText() => CommandTreeExtensions.BuildHelpText(Path);
    }
}