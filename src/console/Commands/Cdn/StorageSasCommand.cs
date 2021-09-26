using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using CommandLine;
using Microsoft.Azure.Storage;

namespace Shamir.Console
{
    public class StorageSasOptions
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

    public sealed class StorageSasCommand : ParsedArgumentsCommand<StorageSasOptions>
    {
        public override string Name => "sas";

        public override string Description => "Generate a SAS token for a file in Storage.";

        public override ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider, StorageSasOptions options)
        {
            var connectionString = options.ConnectionString ?? Environment.GetEnvironmentVariable("AZURE_CONNECTION_STRING");
            var account = CloudStorageAccount.Parse(connectionString);
            
            Debug.Assert(options.Path != null, "Path should be set.");

            var delimiterIndex = options.Path.IndexOf('/');
            var (containerName, path) = delimiterIndex > 0
                ? (options.Path[..delimiterIndex], options.Path[(delimiterIndex + 1)..])
                : (options.Path, string.Empty);

            var client = new BlobServiceClient(connectionString);

            var builder = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddDays(options.ValidityPeriodDays));
            builder.BlobContainerName = containerName;
            builder.BlobName = path;
            builder.Protocol = SasProtocol.Https;
            builder.Resource = "b";

            var key = new StorageSharedKeyCredential(account.Credentials.AccountName, account.Credentials.ExportBase64EncodedKey());
            var parameters = builder.ToSasQueryParameters(key);
            var uri = new UriBuilder
            {
                Scheme = "https",
                Host = options.HostName ?? account.BlobStorageUri.PrimaryUri.Host,
                Path = options.Path,
                Query = parameters.ToString(),
            };

            System.Console.WriteLine(uri.Uri.AbsoluteUri);

            return ValueTask.FromResult(0);
        }
    }
}