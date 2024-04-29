// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a Couchbase container.
/// </summary>
/// <param name="name">The name of the resource.</param>
public class CouchbaseServerResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "tcp";

    private EndpointReference? _primaryEndpoint;

    /// <summary>
    /// Gets the primary endpoint for the Couchbase server.
    /// </summary>
    public EndpointReference PrimaryEndpoint
        => _primaryEndpoint
        ??= new(this, PrimaryEndpointName);

    /// <summary>
    /// Gets the connection string for the Couchbase server
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"couchbases://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}");

    //private readonly Dictionary<string, string> _buckets = new Dictionary<string, string>(StringComparers.ResourceName);

    ///// <summary>
    ///// A dictionary where the key is the resource name and the value is the bucket name.
    ///// </summary>
    //public IReadOnlyDictionary<string, string> Buckets => _buckets;

    //internal void AddBucket(string name, string bucketName)
    //{
    //    _buckets.TryAdd(name, bucketName);
    //}
}
