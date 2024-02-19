using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Telepresence.NET.HeaderPropagation.Mvc.Filters;

/// <summary>
/// Find any headers that pertain to telepresence and store them in the DI container for propagation to downstream.
/// </summary>
/// <remarks>
/// Currently limited to only grabbing headers that contain "x-telepresence" as to avoid conflicts with other technologies.
/// </remarks>
public class TelepresenceActionFilter(TelepresenceContext telepresenceContext) : IActionFilter, IAsyncActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) =>
        Handle(context);

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await Handle(context);
        await next();
    }

    private Task Handle(ActionContext context)
    {
        var headers = context
            .HttpContext
            .Request
            .Headers
            .Where(x => x.Key.Contains("x-telepresence"));

        foreach (var header in headers)
            telepresenceContext.InterceptHeaders.TryAdd(header.Key, header.Value);

        return Task.CompletedTask;
    }
}
