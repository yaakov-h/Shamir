using System;
using System.Collections.Immutable;

namespace Shamir.Console
{
    public interface ICommandTree : IOperationsNode
    {
        ImmutableArray<ICommandTree> SubTrees { get; }
        ImmutableArray<ICommand> Commands { get; }
        ICommand FindCommand(IImmutableStack<ICommandTree> path, ReadOnlySpan<string> args);
    }
}