using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CommandLine;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shamir.Abstractions;

namespace Shamir.Commands.Azure
{
    public sealed class StorageGetUrlOptions
    {
        [Option("connection-string", Required = false, HelpText = "Azure Storage connection string for the Storage Account backing the CDN.")]
        public string? ConnectionString { get; set; }

        [Option('h', "host", Required = false, HelpText = "Hostname to generate the SAS token for.")]
        public string? HostName { get; set; }

        [Option('d', "days", Default = 7, HelpText = "Number of days that the SAS token should remain valid for.")]
        public int ValidityPeriodDays { get; set; }

        [Value(0, MetaName = "path", Required = true, HelpText = "Path to enumerate, starting with the Azure Storage container name.")]
        public string? Path { get; set; }
    }

    public sealed class StorageGetUrlCommand : ParsedArgumentsCommand<StorageGetUrlOptions>
    {
        public override string Name => "get-url";

        public override string Description => "Get the URL for a file in Storage, with a SAS token if required.";

        public override async ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider, StorageGetUrlOptions options)
        {
            var connectionString = options.ConnectionString ?? Environment.GetEnvironmentVariable("AZURE_CONNECTION_STRING");
            var account = CloudStorageAccount.Parse(connectionString);
            
            Debug.Assert(options.Path != null, "Path should be set.");

            var delimiterIndex = options.Path.IndexOf('/');
            var (containerName, path) = delimiterIndex > 0
                ? (options.Path[..delimiterIndex], options.Path[(delimiterIndex + 1)..])
                : (options.Path, string.Empty);

            var uri = new UriBuilder
            {
                Scheme = "https",
                Host = options.HostName ?? account.BlobStorageUri.PrimaryUri.Host,
                Path = options.Path,
            };

            var client = new BlobServiceClient(connectionString);
            var containerPolicy = await client.GetBlobContainerClient(containerName).GetAccessPolicyAsync();
            if (containerPolicy.Value.BlobPublicAccess == PublicAccessType.None)
            {
                var builder = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddDays(options.ValidityPeriodDays));
                builder.BlobContainerName = containerName;
                builder.BlobName = path;
                builder.Protocol = SasProtocol.Https;
                builder.Resource = "b";

                var key = new StorageSharedKeyCredential(account.Credentials.AccountName, account.Credentials.ExportBase64EncodedKey());
                var parameters = builder.ToSasQueryParameters(key);
            
                uri.Query = parameters.ToString();
            }

            serviceProvider.GetRequiredService<IConsole>().Output.WriteLine(uri.Uri.AbsoluteUri);

            return 0;
        }
    }
}