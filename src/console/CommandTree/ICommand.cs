using System;
using System.Threading.Tasks;

namespace Shamir.Console
{
    public interface ICommand : IOperationsNode
    {
        void Initialize(ReadOnlySpan<string> args);
        ValueTask<int> ExecuteAsync(IServiceProvider serviceProvider);
    }
}