using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Shamir.Abstractions;

namespace Shamir.Commands.Radio
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSteamCommandTree(this IServiceCollection collection)
        {
            collection.AddTransient<ICommandTree>(sp => new DefaultCommandTree(
                "steam",
                "Steam Network utilities",
                ImmutableArray<ICommandTree>.Empty,
                ImmutableArray.Create<ICommand>(
                    new DescribeGidCommand()
                )));

            return collection;
        }
    }
}