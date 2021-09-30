using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using Shamir.Abstractions;

namespace Shamir.Commands.Radio
{
    public class VKLookupOptions
    {
        [Value(0, MetaName = "path", Required = true, HelpText = "Callsign to look up.")]
        public string? Callsign { get; set; }
    }

    public sealed class VKLookupCommand : ParsedArgumentsCommand<VKLookupOptions>
    {
        public override string Name => "lookup";

        public override string Description => "Look up an Australian Amateur Radio Callsign";

        public override async ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider, VKLookupOptions options)
        {
            Debug.Assert(options.Callsign != null, "Callsign should be populated.");

            using var client = new HttpClient();

            // API courtesy of VK3FUR: https://vklookup.info
            var baseUri = new Uri("https://l1gfir5yi7.execute-api.us-east-1.amazonaws.com/prod/", UriKind.Absolute);
            var lookupUri = new Uri(baseUri, Uri.EscapeUriString(options.Callsign));

            var response = await client.GetAsync(lookupUri);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"{options.Callsign}: No such callsign found.");
                return 1;
            }

            response.EnsureSuccessStatusCode();

            using var resultStream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(resultStream);

            if (json.RootElement.TryGetProperty("callsign", out var callsign))
            {
                Console.Write("Callsign : ");
                Console.WriteLine(callsign.GetString());
            }

            if (json.RootElement.TryGetProperty("name", out var name))
            {
                Console.Write("Name     : ");
                Console.WriteLine(name.GetString());
            }

            if (json.RootElement.TryGetProperty("suburb", out var suburb))
            {
                Console.Write("Suburb   : ");
                Console.WriteLine(suburb.GetString());
            }

            if (json.RootElement.TryGetProperty("state", out var state))
            {
                Console.Write("State    : ");
                Console.WriteLine(state.GetString());
            }

            if (json.RootElement.TryGetProperty("link", out var link))
            {
                Console.Write("Link     : ");
                Console.WriteLine(link.GetString());
            }

            return 0;
        }
    }
}