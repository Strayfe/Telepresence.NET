using Telepresence.NET.RestfulApi.Models;

namespace Telepresence.NET.RestfulApi;

public interface ITelepresenceApiService
{
    /// <summary>
    /// <para>
    /// Contacts the Telepresence RESTful API `<c>/healthz</c>` endpoint to determine if the API is healthy.
    /// </para>
    /// <para>
    /// If it doesn't then something isn't configured correctly.
    /// </para>
    /// <para>
    /// Check that the traffic-agent container is present and that the `<c>TELEPRESENCE_API_PORT</c>` has been added to
    /// the environment of the application container and/or in the environment that is propagated to the interceptor
    /// that runs on the local workstation.
    /// </para>
    /// </summary>
    /// <returns>
    /// <para>
    /// <c>true</c> (healthy)
    /// </para>
    /// <para>
    /// <c>false</c> (unhealthy or couldn't reach the RESTful API)
    /// </para>
    /// </returns>
    Task<bool> Healthz();

    /// <summary>
    /// <para>
    /// Contacts the Telepresence RESTful API `<c>/consume-here</c>` endpoint to determine if a message should be
    /// consumed.
    /// </para>
    /// <para>
    /// When running in the cluster, this endpoint will respond with false if the headers match an ongoing intercept for
    /// the same workload because it's assumed that it's up to the intercept to consume the message.
    /// </para>
    /// <para>
    /// When running locally, the response is inverted. Matching headers means that the message should be consumed.
    /// </para>
    /// </summary>
    /// <returns>
    /// <para>
    /// <c>true</c> (consume the message)
    /// </para>
    /// <para>
    /// <c>false</c> (leave the message on the queue)
    /// </para>
    /// </returns>
    Task<bool> ConsumeHere(string? optionalPath = null);

    /// <summary>
    /// <para>
    /// Contacts the Telepresence RESTful API `<c>intercept-info</c>` endpoint to get information about a running
    /// intercept.
    /// </para>
    /// </summary>
    Task<InterceptInfo?> InterceptInfo(string? optionalPath = null);
}