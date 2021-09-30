using System;
using System.Collections.Immutable;

namespace Shamir.Abstractions
{
    public sealed class DefaultCommandTree : ICommandTree
    {
        public DefaultCommandTree(string name, string description, ImmutableArray<ICommandTree> subtrees, ImmutableArray<ICommand> commands)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Description = description ?? throw new ArgumentNullException(nameof(description));
            this.SubTrees = subtrees;
            this.Commands = commands;
        }

        public string Name { get; }
        public string Description { get; }
        public ImmutableArray<ICommandTree> SubTrees { get; }
        public ImmutableArray<ICommand> Commands { get; }

        public ICommand FindCommand(IImmutableStack<ICommandTree> path, ReadOnlySpan<string> args)
        {
            if (args.Length > 0)
            {
                foreach (var command in Commands)
                {
                    if (command.Name == args[0])
                    {
                        command.Initialize(args[1..]);
                        return command;
                    }
                }

                foreach (var tree in SubTrees)
                {
                    if (tree.Name == args[0])
                    {
                        return tree.FindCommand(path.Push(this), args[1..]);
                    }
                }
            }

            return new DefaultHelpTextCommand(path.Push(this));
        }
    }
}