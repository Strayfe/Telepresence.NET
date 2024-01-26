using System.Diagnostics;

namespace Telepresence.NET.InterceptSpec.Handlers.ExternalHandlers;

internal interface IExternalHandlerStrategy
{
    Task Handle(Process process, CancellationToken cancellationToken = default);
}