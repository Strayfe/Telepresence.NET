using System.Diagnostics;

namespace Telepresence.NET.Models.Intercept.Handlers.ExternalHandlers;

internal interface IExternalHandlerStrategy
{
    Task Handle(Process process, CancellationToken cancellationToken = default);
}