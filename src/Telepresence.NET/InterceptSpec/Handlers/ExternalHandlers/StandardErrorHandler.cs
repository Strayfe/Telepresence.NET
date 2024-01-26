using System.Diagnostics;

namespace Telepresence.NET.InterceptSpec.Handlers.ExternalHandlers;

internal class StandardErrorHandler : IExternalHandlerStrategy
{
    public async Task Handle(Process process, CancellationToken cancellationToken = default)
    {
        // I am not yet sure why anyone would want to use stderr as their handler
        throw new NotImplementedException();
    }
}