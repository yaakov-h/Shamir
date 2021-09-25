using System.Threading.Tasks;

namespace Shamir.Console
{
    public interface ICommand : IOperationsNode
    {
        ValueTask<int> ExecuteAsync();
    }
}