using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace Shamir.Console
{
    public class CdnOperations : ICommandSet
    {
        [Verb("ls", HelpText = "List files on the CDN")]
        public class CdnLsOptions
        {
        }

        [Verb("add", HelpText = "Upload a file to the CDN")]
        public class CdnAddOptions
        {
        }

        [Verb("mv", HelpText = "Move a file to the CDN")]
        public class CdnMvOptions
        {
        }

        public static IEnumerable<Type> VerbOptionTypes
        {
            get
            {
                yield return typeof(CdnLsOptions);
                yield return typeof(CdnAddOptions);
                yield return typeof(CdnMvOptions);
            }
        }

        public ICommand? FindCommand(ReadOnlySpan<string> args)
        {
            if (args.Length == 0) return null;

            var parserResult = Parser.Default.ParseArguments<CdnLsOptions, CdnAddOptions, CdnMvOptions>(args.ToArray());
            return parserResult.MapResult<CdnLsOptions, CdnAddOptions, CdnMvOptions, ICommand>(
                options => new CdnLsCommand(options),
                options => new CdnAddCommand(options),
                options => new CdnMvCommand(options),
                errors => new HelpTextCommand(parserResult)
            );
        }

        public class CdnLsCommand : ICommand
        {
            public CdnLsCommand(CdnLsOptions options)
            {
            }

            public ValueTask<int> ExecuteAsync()
            {
                Console.WriteLine($"Executing: cdn-ls");
                return ValueTask.FromResult(0);
            }
        }

        public class CdnAddCommand : ICommand
        {
            public CdnAddCommand(CdnAddOptions options)
            {
            }
            
            public ValueTask<int> ExecuteAsync()
            {
                Console.WriteLine($"Executing: cdn-add");
                return ValueTask.FromResult(0);
            }
        }

        public class CdnMvCommand : ICommand
        {
            public CdnMvCommand(CdnMvOptions options)
            {
            }
            
            public ValueTask<int> ExecuteAsync()
            {
                Console.WriteLine($"Executing: cdn-mv");
                return ValueTask.FromResult(0);
            }
        }

        public class HelpTextCommand : ICommand
        {
            public HelpTextCommand(ParserResult<object> result)
            {
                this.result = result ?? throw new ArgumentNullException(nameof(result));
                Debug.Assert(result.Tag == ParserResultType.NotParsed);
            }

            readonly ParserResult<object> result;

            public ValueTask<int> ExecuteAsync()
            {
                var helpText = HelpText.AutoBuild(result);
                Console.Error.WriteLine(helpText);
                return ValueTask.FromResult(1);
            }
        }
    }

    public interface ICommand
    {
        ValueTask<int> ExecuteAsync();
    }

    public interface ICommandSet
    {
        ICommand? FindCommand(ReadOnlySpan<string> args);
        static IEnumerable<Type> VerbOptionTypes { get; }
    }

    public class CommandTree : ICommandSet
    {
        IEnumerable<Type> VerbOptionTypes { get; }
        public ICommand? FindCommand(ReadOnlySpan<string> args)
        {
            if (args.Length == 0) return null;

            return args[0] switch {
                "cdn" => new CdnOperations().FindCommand(args[1..]),
                _ => null,
            };
        }
    }

    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var tree = new CommandTree();
            var command = tree.FindCommand(args);
            if (command is null)
            {
                PrintHelpText(tree);
                return 1;
            }

            return await command.ExecuteAsync();
        }
        
        static void PrintHelpText(ICommandSet tree)
        {

        }
    }
}
