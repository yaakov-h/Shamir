using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Shamir.Abstractions;

namespace Shamir.Commands.Azure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureCommandTree(this IServiceCollection collection)
        {
            collection.AddTransient<ICommandTree>(sp => new DefaultCommandTree(
                "cdn",
                "Browse and add new data to a CDN backed by Azure Storage",
                ImmutableArray<ICommandTree>.Empty,
                ImmutableArray.Create<ICommand>(
                    new StorageLsCommand(),
                    new StorageCopyCommand(),
                    new StorageGetUrlCommand()
                )));

            return collection;
        }
    }
}