// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Couchbase;
using Couchbase.Management.Buckets;

namespace Aspire.Couchbase;

public interface ICouchbaseClient
{
    ClusterOptions ClusterOptions { get; }
    Cluster? Cluster { get; }

    Dictionary<string, BucketSettings>? ListBuckets(CancellationToken cancellationToken);
}

public sealed class CouchbaseClient
    : ICouchbaseClient
{
    public ClusterOptions ClusterOptions { get; }
    public Cluster? Cluster { get; }

    public CouchbaseClient(ClusterOptions clusterOptions)
    {
        ClusterOptions = clusterOptions;
    }

    public Dictionary<string, BucketSettings>? ListBuckets(CancellationToken cancellationToken)
    {
        return Cluster?.Buckets
            .GetAllBucketsAsync()
            .GetAwaiter()
            .GetResult();
    }
}
