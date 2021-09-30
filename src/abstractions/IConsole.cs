using System.IO;

namespace Shamir.Abstractions
{
    public interface IConsole
    {
        TextWriter Output { get; }
        TextWriter Error { get; }
        Stream OpenInput();
    }
}