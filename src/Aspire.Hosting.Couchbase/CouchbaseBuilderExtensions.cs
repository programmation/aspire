// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Couchbase;
using Aspire.Hosting.Utils;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding Couchbase resources to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class CouchbaseBuilderExtensions
{
    // Internal port is always 8091.
    private const int DefaultContainerPort = 8091;

    /// <summary>
    /// Adds a Couchbase resource to the application model. A container is used for local development. This version the package defaults to the 7.3.1 tag of the Couchbase container image.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="port">The host port for Couchbase.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<CouchbaseServerResource> AddCouchbase(this IDistributedApplicationBuilder builder, string name, int? port = null)
    {
        var couchbaseContainer = new CouchbaseServerResource(name);

        return builder
            .AddResource(couchbaseContainer)
            .WithEndpoint(port: port, targetPort: DefaultContainerPort, name: CouchbaseServerResource.PrimaryEndpointName)
            .WithImage(CouchbaseContainerImageTags.Image, CouchbaseContainerImageTags.Tag)
            .WithImageRegistry(CouchbaseContainerImageTags.Registry);
    }

    /// <summary>
    /// Adds a Couchbase Sync Gateway platform to the application model. This version the package defaults to the 1.0.2-20 tag of the mongo-express container image
    /// </summary>
    /// <param name="builder">The Couchbase server resource builder.</param>
    /// <param name="configureContainer">Configuration callback for Couchbase Sync Gateway container resource.</param>
    /// <param name="containerName">The name of the container (Optional).</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithSyncGateway<T>(this IResourceBuilder<T> builder, Action<IResourceBuilder<CouchbaseSyncGatewayContainerResource>>? configureContainer = null, string? containerName = null) where T : CouchbaseServerResource
    {
        containerName ??= $"{builder.Resource.Name}-sync-gateway";

        var syncGatewayContainer = new CouchbaseSyncGatewayContainerResource(containerName);
        var resourceBuilder = builder.ApplicationBuilder.AddResource(syncGatewayContainer)
                                                        .WithImage(CouchbaseContainerImageTags.SyncGatewayImage, CouchbaseContainerImageTags.SyncGatewayTag)
                                                        .WithImageRegistry(CouchbaseContainerImageTags.SyncGatewayRegistry)
                                                        //.WithEnvironment(context => ConfigureMongoExpressContainer(context, builder.Resource))
                                                        .WithHttpEndpoint(targetPort: 4984, name: "http")
                                                        .ExcludeFromManifest();

        configureContainer?.Invoke(resourceBuilder);

        return builder;
    }

    /// <summary>
    /// Adds a named volume for the data folder to a Couchbase container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<CouchbaseServerResource> WithDataVolume(this IResourceBuilder<CouchbaseServerResource> builder, string? name = null, bool isReadOnly = false)
        => builder.WithVolume(name ?? VolumeNameGenerator.CreateVolumeName(builder, "data"), "/data/db", isReadOnly);

    /// <summary>
    /// Adds a bind mount for the data folder to a Couchbase container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<CouchbaseServerResource> WithDataBindMount(this IResourceBuilder<CouchbaseServerResource> builder, string source, bool isReadOnly = false)
        => builder.WithBindMount(source, "/data/db", isReadOnly);

    /// <summary>
    /// Adds a bind mount for the init folder to a Couchbase container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<CouchbaseServerResource> WithInitBindMount(this IResourceBuilder<CouchbaseServerResource> builder, string source, bool isReadOnly = true)
        => builder.WithBindMount(source, "/docker-entrypoint-initdb.d", isReadOnly);
}
