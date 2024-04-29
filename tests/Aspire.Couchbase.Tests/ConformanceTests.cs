// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Components.Common.Tests;
using Aspire.Components.ConformanceTests;
//using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Aspire.Couchbase.Tests;

public class ConformanceTests : ConformanceTests<ICouchbaseClient, CouchbaseSettings>, IClassFixture<CouchbaseContainerFixture>
{
    private readonly CouchbaseContainerFixture _containerFixture;

    protected override ServiceLifetime ServiceLifetime => ServiceLifetime.Singleton;

    protected override string ActivitySourceName => "Couchbase.Extensions.DiagnosticSources";

    protected override bool SupportsKeyedRegistrations => true;

    protected override bool CanConnectToServer => RequiresDockerTheoryAttribute.IsSupported;

    protected override string ValidJsonConfig => """
        {
          "Aspire": {
            "Couchbase": {
              "ConnectionString": "YOUR_CONNECTION_STRING",
              "DisableHealthChecks": false,
              "HealthCheckTimeout": 100,
              "DisableTracing": false
            }
          }
        }
        """;

    public ConformanceTests(CouchbaseContainerFixture containerFixture)
    {
        _containerFixture = containerFixture;
    }

    protected override (string json, string error)[] InvalidJsonToErrorMessage => new[]
    {
        ("""{"Aspire": { "Couchbase": { "DisableHealthChecks": "true"}}}""", "Value is \"string\" but should be \"boolean\""),
        ("""{"Aspire": { "Couchbase": { "HealthCheckTimeout": "10000"}}}""", "Value is \"string\" but should be \"integer\""),
        ("""{"Aspire": { "Couchbase": { "DisableTracing": "true"}}}""", "Value is \"string\" but should be \"boolean\""),
    };

    // TODO
    protected override string[] RequiredLogCategories => [
        //"Couchbase.Connection",
    ];

    protected override void PopulateConfiguration(ConfigurationManager configuration, string? key = null)
    {
        var connectionString = RequiresDockerTheoryAttribute.IsSupported ?
            $"{_containerFixture.GetConnectionString()}" :
            "couchbases://userid@localhost:27017";

        configuration.AddInMemoryCollection(
            [
            new KeyValuePair<string, string?>(
                CreateConfigKey("Aspire:Couchbase", key, "ConnectionString"),
                connectionString)
            ]);
    }

    protected override void RegisterComponent(HostApplicationBuilder builder, Action<CouchbaseSettings>? configure = null, string? key = null)
    {
        if (key is null)
        {
            builder.AddCouchbaseClient("couchbases", configure);
        }
        else
        {
            builder.AddKeyedCouchbaseClient(key, configure);
        }
    }

    // TODO
    protected override void SetHealthCheck(CouchbaseSettings options, bool enabled)
        => throw new NotImplementedException();
    //{
    //    options.DisableHealthChecks = true; // !enabled;
    //    options.HealthCheckTimeout = 10;
    //}

    // TODO
    protected override void SetMetrics(CouchbaseSettings options, bool enabled)
        => throw new NotImplementedException();

    protected override void SetTracing(CouchbaseSettings options, bool enabled)
    {
        options.DisableTracing = !enabled;
    }

    protected override void TriggerActivity(ICouchbaseClient service)
    {
        using var source = new CancellationTokenSource(10);

        service.ListBuckets(source.Token);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("key")]
    public void ClientAndDatabaseInstancesShouldBeResolved(string? key)
    {
        using var host = CreateHostWithComponent(key: key);

        var couchbaseClient = Resolve<ICouchbaseClient>();
        //ICouchbaseCluster? couchbaseCluster = Resolver<ICouchbaseCluster>();

        Assert.NotNull(couchbaseClient);
        //Assert.NotNull(couchbaseCluster);

        T? Resolve<T>() => key is null
            ? host.Services.GetService<T>()
            : host.Services.GetKeyedService<T>(key);
    }
}
