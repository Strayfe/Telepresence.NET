using System.Diagnostics;

namespace Telepresence.NET.InterceptSpec.Handlers;

internal class Docker : IHandlerStrategy
{
    public async Task Handle(Process process, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}