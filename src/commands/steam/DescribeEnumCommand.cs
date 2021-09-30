using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Shamir.Abstractions;
using SteamKit2;

namespace Shamir.Commands.Radio
{
    public class DescribeEnumOptions
    {
        [Value(0, MetaName = "enum", Required = true, HelpText = "Name of the enum to search.")]
        public string? EnumName { get; set; }

        [Value(0, MetaName = "value", Required = true, HelpText = "Query (substring or numeric value).")]
        public string? EnumValue { get; set; }
    }

    public sealed class DescribeEnumCommand : ParsedArgumentsCommand<DescribeEnumOptions>
    {
        public override string Name => "enum";

        public override string Description => "Find a Steam enum value";

        public override ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider, DescribeEnumOptions options)
        {
            Debug.Assert(options.EnumName != null, "Value should be set before execution.");
            Debug.Assert(options.EnumValue != null, "Value should be set before execution.");

            var console = serviceProvider.GetRequiredService<IConsole>();

            var enumType = FindEnum(options.EnumName);

            if (enumType is null)
            {
                console.Error.WriteLine("No such enum could be find. Valid names:");

                foreach (var @enum in SalientEnums.OrderBy(x => x))
                {
                    console.Error.Write("  - ");
                    console.Error.WriteLine(@enum.Name);
                }

                return ValueTask.FromResult(1);
            }

            if (Enum.TryParse(enumType, options.EnumValue, ignoreCase: true, out var result))
            {
                console.Output.WriteLine($"{result:G} = {result:D}");
                return ValueTask.FromResult(0);
            }
            
            var names = Enum.GetNames(enumType);
            var matchingNames = names.Where(x => x.IndexOf(options.EnumValue, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            switch (matchingNames.Count)
            {
                case 0:
                    console.Error.WriteLine($"No match found in ${enumType.Name} for '{options.EnumValue}'");
                    return ValueTask.FromResult(1);

                case 1:
                    result = enumType.GetField(matchingNames[0], BindingFlags.Public | BindingFlags.Static)!.GetValue(null);
                    console.Output.WriteLine($"{result:G} = {result:D}");
                    return ValueTask.FromResult(0);

                default:
                    console.Error.WriteLine($"Multiple matches found in {enumType.Name}:");
                    foreach (var match in matchingNames.OrderBy(x => x))
                    {
                        console.Error.Write("  - ");
                        console.Error.WriteLine(match);
                    }
                    return ValueTask.FromResult(1);
            }
        }

        static Type? FindEnum(string name)
        {
            foreach (var @enum in SalientEnums)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(@enum.Name, name))
                {
                    return @enum;
                }
            }

            return null;
        }
        
        static ImmutableArray<Type> SalientEnums { get; } = ImmutableArray.Create(
            typeof(EAccountFlags),
            typeof(EAccountType),
            typeof(EAppType),
            typeof(EClanPermission),
            typeof(EClanRank),
            typeof(ECurrencyCode),
            typeof(EMsg),
            typeof(EPaymentMethod),
            typeof(EPersonaState),
            typeof(EPersonaStateFlag),
            typeof(EResult),
            typeof(EOSType));
    }
}