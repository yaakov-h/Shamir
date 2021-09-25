using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Shamir.Console
{
    public abstract class ParsedArgumentsCommand<TOptions> : ICommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public ImmutableArray<string> Arguments { get; private set; }

        public void Initialize(ReadOnlySpan<string> args)
        {
            var array = ImmutableArray.CreateBuilder<string>(args.Length);
            foreach (var arg in args)
            {
                array.Add(arg);
            }
            Arguments = array.MoveToImmutable();
        }

        public async ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider)
        {
            using var parser = serviceProvider.GetRequiredService<Parser>();
            var result = parser.ParseArguments<TOptions>(Arguments);
            return await result.MapResult(
                options => ExecuteAsync(serviceProvider, options),
                errors => ValueTask.FromResult(1)
            );
        }

        public abstract ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider, TOptions options);
    }

    public class StorageLsOptions
    {
        [Option("connection-string", Required = false)]
        public string? ConnectionString { get; set; }

        [Value(0)]
        public string? ContainerName { get; set; }
    }

    public sealed class StorageLsCommand : ParsedArgumentsCommand<StorageLsOptions>
    {
        public override string Name => "ls";

        public override string Description => "List files in an Azure Storage account.";

        public override async ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider, StorageLsOptions options)
        {
            var connectionString = options.ConnectionString ?? Environment.GetEnvironmentVariable("AZURE_CONNECTION_STRING");
            
            if (options.ContainerName is null)
            {
                var client = new BlobServiceClient(connectionString);
                await foreach (var container in client.GetBlobContainersAsync())
                {
                    System.Console.WriteLine(container.Name);
                }
            }
            else
            {
                var client = new BlobContainerClient(connectionString, options.ContainerName);

                await foreach (var blob in client.GetBlobsAsync())
                {
                    System.Console.Write(options.ContainerName);
                    System.Console.Write('/');
                    System.Console.WriteLine(blob.Name);
                }
            }

            return 0;
        }
    }
}