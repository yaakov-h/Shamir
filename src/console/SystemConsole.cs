using System.IO;
using Shamir.Abstractions;

namespace Shamir.Console
{
    public sealed class SystemConsole : IConsole
    {
        public TextWriter Output => System.Console.Out;

        public TextWriter Error => System.Console.Error;

        public Stream OpenInput() => System.Console.OpenStandardInput();
    }
}