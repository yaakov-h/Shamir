using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Shamir.Abstractions;

namespace Shamir.Commands.Radio
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRadioCommandTree(this IServiceCollection collection)
        {
            collection.AddTransient<ICommandTree>(sp => new DefaultCommandTree(
                "vk",
                "Australian Amateur Radio commands",
                ImmutableArray<ICommandTree>.Empty,
                ImmutableArray.Create<ICommand>(
                    new VKLookupCommand()
                )));

            return collection;
        }
    }
}