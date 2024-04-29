// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.Couchbase;

/// <summary>
/// A resource that represents a Couchbase Sync Gateway container.
/// </summary>
/// <param name="name">The name of the resource.</param>
public sealed class CouchbaseSyncGatewayContainerResource(string name) : ContainerResource(name)
{
}
