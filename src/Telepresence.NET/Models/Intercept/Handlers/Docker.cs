using System.Diagnostics;

namespace Telepresence.NET.Models.Intercept.Handlers;

public class Docker : IHandlerStrategy
{
    public async Task Handle(Process process, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}