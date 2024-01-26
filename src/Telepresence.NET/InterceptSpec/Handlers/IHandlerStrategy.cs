using System.Diagnostics;

namespace Telepresence.NET.InterceptSpec.Handlers;

internal interface IHandlerStrategy
{
    Task Handle(Process process, CancellationToken cancellationToken = default);
}