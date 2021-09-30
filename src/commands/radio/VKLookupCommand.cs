using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
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

            var console = serviceProvider.GetRequiredService<IConsole>();
            using var client = new HttpClient();

            // API courtesy of VK3FUR: https://vklookup.info
            var baseUri = new Uri("https://l1gfir5yi7.execute-api.us-east-1.amazonaws.com/prod/", UriKind.Absolute);
            var lookupUri = new Uri(baseUri, Uri.EscapeUriString(options.Callsign));

            var response = await client.GetAsync(lookupUri);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                console.Error.WriteLine($"{options.Callsign}: No such callsign found.");
                return 1;
            }

            response.EnsureSuccessStatusCode();

            using var resultStream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(resultStream);

            if (json.RootElement.TryGetProperty("callsign", out var callsign))
            {
                console.Output.Write("Callsign : ");
                console.Output.WriteLine(callsign.GetString());
            }

            if (json.RootElement.TryGetProperty("name", out var name))
            {
                console.Output.Write("Name     : ");
                console.Output.WriteLine(name.GetString());
            }

            if (json.RootElement.TryGetProperty("suburb", out var suburb))
            {
                console.Output.Write("Suburb   : ");
                console.Output.WriteLine(suburb.GetString());
            }

            if (json.RootElement.TryGetProperty("state", out var state))
            {
                console.Output.Write("State    : ");
                console.Output.WriteLine(state.GetString());
            }

            if (json.RootElement.TryGetProperty("link", out var link))
            {
                console.Output.Write("Link     : ");
                console.Output.WriteLine(link.GetString());
            }

            return 0;
        }
    }
}