using System;
using System.Collections.Immutable;
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

            public void Initialize(ReadOnlySpan<string> args) { }
            public ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider) => ValueTask.FromResult(0);
        }
    }
}