// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Components.Common.Tests;
using Aspire.Hosting.Couchbase;
using Testcontainers.Couchbase;
using Xunit;

namespace Aspire.Couchbase.Tests;

public sealed class CouchbaseContainerFixture : IAsyncLifetime
{
    public CouchbaseContainer? Container { get; private set; }

    public string GetConnectionString() => Container?.GetConnectionString() ??
        throw new InvalidOperationException("The test container was not initialized.");

    public async Task InitializeAsync()
    {
        if (RequiresDockerTheoryAttribute.IsSupported)
        {
            // testcontainers uses mongo:mongo by default,
            // resetting that for tests
            Container = new CouchbaseBuilder()
                .WithImage($"{CouchbaseContainerImageTags.Image}:{CouchbaseContainerImageTags.Tag}")
                //.WithUsername(null)
                //.WithPassword(null)
                .Build();
            await Container.StartAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (Container is not null)
        {
            await Container.DisposeAsync();
        }
    }
}
