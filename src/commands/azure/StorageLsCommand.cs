using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Shamir.Abstractions;

namespace Shamir.Commands.Azure
{
    public sealed class StorageLsOptions
    {
        [Option("connection-string", Required = false, HelpText = "Azure Storage connection string for the Storage Account backing the CDN.")]
        public string? ConnectionString { get; set; }

        [Option('a', "all", HelpText = "List all blobs in the container")]
        public bool EnumerateAll { get; set; }

        [Value(0, MetaName = "path", HelpText = "Path to enumerate, starting with the Azure Storage container name.")]
        public string? Path { get; set; }
    }

    public sealed class StorageLsCommand : ParsedArgumentsCommand<StorageLsOptions>
    {
        public override string Name => "ls";

        public override string Description => "List files in an Azure Storage account.";

        public override async ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider, StorageLsOptions options)
        {
            var connectionString = options.ConnectionString ?? Environment.GetEnvironmentVariable("AZURE_CONNECTION_STRING");
            var console = serviceProvider.GetRequiredService<IConsole>();
            
            if (options.Path is null)
            {
                if (options.EnumerateAll)
                {
                    var client = new BlobServiceClient(connectionString);
                    await foreach (var container in client.GetBlobContainersAsync())
                    {
                        var containerClient = new BlobContainerClient(connectionString, container.Name);

                        await foreach (var blob in containerClient.GetBlobsAsync())
                        {
                            console.Output.Write(container.Name);
                            console.Output.Write('/');
                            console.Output.WriteLine(blob.Name);
                        }
                    }
                }
                else
                {
                    var client = new BlobServiceClient(connectionString);
                    await foreach (var container in client.GetBlobContainersAsync())
                    {
                        console.Output.WriteLine(container.Name);
                    }
                }
            }
            else if (options.EnumerateAll)
            {
                var containerName = options.Path;
                var client = new BlobContainerClient(connectionString, containerName);

                await foreach (var blob in client.GetBlobsAsync())
                {
                    console.Output.Write(containerName);
                    console.Output.Write('/');
                    console.Output.WriteLine(blob.Name);
                }
            }
            else
            {
                var delimiterIndex = options.Path.IndexOf('/');
                var (containerName, prefix) = delimiterIndex > 0
                    ? (options.Path[..delimiterIndex], options.Path[(delimiterIndex + 1)..])
                    : (options.Path, string.Empty);

                var client = new BlobContainerClient(connectionString, containerName);
                await foreach (var blob in client.GetBlobsByHierarchyAsync(default, default, delimiter: "/", prefix))
                {
                    console.Output.Write(containerName);
                    console.Output.Write('/');

                    if (blob.IsPrefix)
                    {
                        console.Output.WriteLine(blob.Prefix);
                    }
                    else if (blob.IsBlob)
                    {
                        console.Output.WriteLine(blob.Blob.Name);
                    }
                    else
                    {
                        throw new NotImplementedException("Unknown result - neither a blob nor a prefix (folder).");
                    }
                }
            }

            return 0;
        }
    }
}