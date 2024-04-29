// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Hosting.Couchbase;

internal static class CouchbaseContainerImageTags
{
    public const string Registry = "docker.io";
    public const string Image = "library/couchbase";
    public const string Tag = "7.6.1";
    public const string SyncGatewayRegistry = "docker.io";
    public const string SyncGatewayImage = "library/couchbase/sync-gateway";
    public const string SyncGatewayTag = "3.1.5-enterprise";
}
