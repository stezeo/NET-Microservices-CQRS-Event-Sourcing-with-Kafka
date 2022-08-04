using CQRS.Core.Commands;
using System;
using System.Threading.Tasks;

namespace CQRS.Core.Infrastructure
{
    public interface ICommandDispatcher
    {
        void RegisterHandler<T>(Func<T, Task> handler) where T : BaseCommand;
        Task SendAsync(BaseCommand command);
    }
}