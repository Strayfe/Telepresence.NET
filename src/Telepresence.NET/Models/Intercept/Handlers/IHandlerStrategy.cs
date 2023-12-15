using System.Diagnostics;

namespace Telepresence.NET.Models.Intercept.Handlers;

internal interface IHandlerStrategy
{
    Task Handle(Process process, CancellationToken cancellationToken = default);
}