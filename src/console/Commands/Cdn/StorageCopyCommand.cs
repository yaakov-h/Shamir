using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CommandLine;

namespace Shamir.Console
{
    public class StorageCopyOptions
    {
        [Option("connection-string", Required = false, HelpText = "Azure Storage connection string for the Storage Account backing the CDN.")]
        public string? ConnectionString { get; set; }

        [Value(0, Required = true, MetaName = "LocalPath", HelpText = "The local path to the file to copy.")]
        public string? LocalPath { get; set; }

        [Value(1, Required = true, MetaName = "RemotePath", HelpText = "The destination path in Azure Storage.")]
        public string? RemotePath { get; set; }
    }

    public sealed class StorageCopyCommand : ParsedArgumentsCommand<StorageCopyOptions>
    {
        public override string Name => "cp";

        public override string Description => "Copy a file to an Azure Storage account.";

        public override async ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider, StorageCopyOptions options)
        {
            var connectionString = options.ConnectionString ?? Environment.GetEnvironmentVariable("AZURE_CONNECTION_STRING");

            Debug.Assert(options.LocalPath != null, "LocalPath should be set.");
            Debug.Assert(options.RemotePath != null, "RemotePath should be set.");

            var delimiterIndex = options.RemotePath.IndexOf('/');
            var (containerName, path) = delimiterIndex > 0
                ? (options.RemotePath[..delimiterIndex], options.RemotePath[(delimiterIndex + 1)..])
                : (options.RemotePath, string.Empty);

            var inputIsStdInputStream = options.LocalPath == "-";

            if (string.IsNullOrEmpty(path))
            {
                if (inputIsStdInputStream)
                {
                    throw new NotSupportedException("The remote path must include a name for the blob, as it cannot be derived from the local file name.");
                }

                path = Path.GetFileName(options.LocalPath);
            }

            var client = new BlobContainerClient(connectionString, containerName);
            await client.CreateIfNotExistsAsync(PublicAccessType.Blob);

            using var blobStream = inputIsStdInputStream ? System.Console.OpenStandardInput() : File.OpenRead(options.LocalPath);
            var response = await client.UploadBlobAsync(path, blobStream);

            return 0;
        }
    }
}