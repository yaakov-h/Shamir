using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Shamir.Console.Tests
{
    public class CommandTreeTests
    {
        [SetUp]
        public void Setup()
        {
            tree = new DefaultCommandTree(
                "root",
                "top level",
                ImmutableArray.Create<ICommandTree>(
                    new DefaultCommandTree(
                        "bar",
                        "child command tree",
                        ImmutableArray<ICommandTree>.Empty,
                        ImmutableArray.Create<ICommand>(
                            new SimpleTextCommand("one", "bar/1 command"),
                            new SimpleTextCommand("two", "bar/2 command")
                        ))),
                ImmutableArray.Create<ICommand>(
                    new SimpleTextCommand("foo", "foo command")
                ));
        }

        ICommandTree tree;

        [Test]
        public void ConstructsHelpTextWithNoArgs()
        {
            var command = tree.FindCommand(Array.Empty<string>());
            Assert.That(command, Is.InstanceOf<DefaultHelpTextCommand>());
            var helpText = ((DefaultHelpTextCommand)command).GetHelpText();
            Assert.That(helpText, Is.EqualTo(@"
Group
    root : top level

Subgroups:
    bar : child command tree

Commands:
    foo : foo command
".TrimStart()));
        }
        
        [Test]
        public void ConstructsHelpTextForSubTree()
        {
            var command = tree.FindCommand(new[] { "bar" });
            Assert.That(command, Is.InstanceOf<DefaultHelpTextCommand>());
            var helpText = ((DefaultHelpTextCommand)command).GetHelpText();
            Assert.That(helpText, Is.EqualTo(@"
Group
    root bar : child command tree

Commands:
    one : bar/1 command
    two : bar/2 command
".TrimStart()));
        }

        [TestCase((object)new string[] { "foo" }, ExpectedResult = "foo command")]
        [TestCase((object)new string[] { "bar", "one" }, ExpectedResult = "bar/1 command")]
        [TestCase((object)new string[] { "bar", "two" }, ExpectedResult = "bar/2 command")]
        public string FindsCommand(string[] args)
        {
            var command = tree.FindCommand(ImmutableStack<ICommandTree>.Empty, args);
            return command?.Description;
        }


        class SimpleTextCommand : ICommand
        {
            public SimpleTextCommand(string name, string description)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Description = description ?? throw new ArgumentNullException(nameof(description));
            }

            public string Name { get; }

            public string Description { get; }

            public ValueTask<int> ExecuteAsync()
            {
                return ValueTask.FromResult(0);
            }
        }
    }

    interface IOperationsNode
    {
        string Name { get; }
        string Description { get; }
    }

    interface ICommand : IOperationsNode
    {
        ValueTask<int> ExecuteAsync();
    }

    interface ICommandTree : IOperationsNode
    {
        ImmutableArray<ICommandTree> SubTrees { get; }
        ImmutableArray<ICommand> Commands { get; }
        ICommand FindCommand(IImmutableStack<ICommandTree> path, ReadOnlySpan<string> args);
    }

    class DefaultCommandTree : ICommandTree
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

    class DefaultHelpTextCommand : ICommand
    {
        public DefaultHelpTextCommand(IImmutableStack<ICommandTree> path)
        {
            this.Path = path;
        }

        public string Name => "help";
        public string Description => "Print help text";

        public IImmutableStack<ICommandTree> Path { get; }

        public ValueTask<int> ExecuteAsync()
        {
            System.Console.Error.WriteLine(GetHelpText());
            return ValueTask.FromResult(1); // TODO: const
        }

        public string GetHelpText() => CommandTreeExtensions.BuildHelpText(Path);
    }

    static class CommandTreeExtensions
    {
        public static ICommand? FindCommand(this ICommandTree tree, ReadOnlySpan<string> args)
        {
            if (tree is null)
            {
                throw new ArgumentNullException(nameof(tree));
            }

            return tree.FindCommand(ImmutableStack<ICommandTree>.Empty, args);
        }

        public static string BuildHelpText(IImmutableStack<ICommandTree> path)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Group");
            sb.Append("    ");

            var pathNodes = path.ToArray();
            for (var i = pathNodes.Length - 1; i > 0; i--)
            {
                sb.Append(pathNodes[i].Name);
                sb.Append(' ');
            }

            var tree = pathNodes[0];
            sb.Append(tree.Name);
            sb.Append(" : ");
            sb.AppendLine(tree.Description);
            sb.AppendLine();

            if (!tree.SubTrees.IsEmpty)
            {
                sb.AppendLine("Subgroups:");

                var maxSpacing = tree.SubTrees.Max(c => c.Name.Length) + 1;
                
                foreach (var child in tree.SubTrees)
                {
                    sb.Append("    ");
                    sb.Append(child.Name);
                    for (var i = 0; i < maxSpacing - child.Name.Length; i++)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(": ");
                    sb.AppendLine(child.Description);
                }

                sb.AppendLine();
            }

            if (!tree.Commands.IsEmpty)
            {
                sb.AppendLine("Commands:");

                var maxSpacing = tree.Commands.Max(c => c.Name.Length) + 1;
                
                foreach (var command in tree.Commands)
                {
                    sb.Append("    ");
                    sb.Append(command.Name);
                    for (var i = 0; i < maxSpacing - command.Name.Length; i++)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(": ");
                    sb.AppendLine(command.Description);
                }
            }
            
            return sb.ToString();
        }
    }
}