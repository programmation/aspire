// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire;
using Aspire.Couchbase;
using Couchbase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting;

public static class AspireCouchbaseExtensions
{
    private const string DefaultConfigSectionName = "Aspire:Couchbase";
    private const string ActivityNameSource = "Couchbase.Extensions.DiagnosticSources";

    public static void AddCouchbaseClient(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<CouchbaseSettings>? configureSettings = null,
        Action<ClusterOptions>? configureClusterOptions = null)
        => builder.AddCouchbaseClient(DefaultConfigSectionName, configureSettings, configureClusterOptions, connectionName, serviceKey: null);

    public static void AddKeyedCouchbaseClient(
        this IHostApplicationBuilder builder,
        string name,
        Action<CouchbaseSettings>? configureSettings = null,
        Action<ClusterOptions>? configureClusterOptions = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        
        builder.AddCouchbaseClient(
            $"{DefaultConfigSectionName}:{name}",
            configureSettings,
            configureClusterOptions,
            connectionName: name,
            serviceKey: name
            );
    }

    private static void AddCouchbaseClient(
        this IHostApplicationBuilder builder,
        string configurationSectionName,
        Action<CouchbaseSettings>? configureSettings,
        Action<ClusterOptions>? configureClusterOptions,
        string connectionName,
        object? serviceKey)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var settings = builder.GetCouchbaseSettings(
            connectionName,
            configurationSectionName,
            configureSettings);

        builder.AddCouchbaseClient(
            settings,
            connectionName,
            configurationSectionName,
            configureClusterOptions,
            serviceKey);

        if (!settings.DisableTracing)
        {
            builder.Services
                .AddOpenTelemetry()
                .WithTracing(tracer => tracer.AddSource(ActivityNameSource));
        }

        //builder.AddCouchbaseBucket(settings.ConnectionString, serviceKey);
        //builder.AddHealthCheck(
        //    serviceKey is null ? "Couchbase" : $"Couchbase_{connectionName}",
        //    settings);
    }

    private static void AddCouchbaseClient(
        this IHostApplicationBuilder builder,
        CouchbaseSettings couchbaseSettings,
        string connectionName,
        string configurationSectionName,
        Action<ClusterOptions>? configureClusterOptions,
        object? serviceKey)
    {
        if (serviceKey is null)
        {
            builder.Services
                .AddSingleton<ICouchbaseClient>(sp => sp.CreateCouchbaseClient(connectionName, configurationSectionName, couchbaseSettings, configureClusterOptions));
            return;
        }

        builder.Services
            .AddKeyedSingleton<ICouchbaseClient>(serviceKey, (sp, _) => sp.CreateCouchbaseClient(connectionName, configurationSectionName, couchbaseSettings, configureClusterOptions));
    }

    //private static void AddCouchbaseBucket(
    //    this IHostApplicationBuilder builder,
    //    string? connectionString,
    //    object? serviceKey = null)
    //{
    //    if (string.IsNullOrWhiteSpace(connectionString))
    //    {
    //        return;
    //    }

    //    if (serviceKey is null)
    //    {
    //        builder .Services.addsi
    //    }
    //}

    //private static void AddHealthCheck(
    //    this IHostApplicationBuilder builder,
    //    string healthCheckName,
    //    CouchbaseSettings settings)
    //{
    //    if (settings.DisableHealthChecks || string.IsNullOrWhiteSpace(settings.ConnectionString))
    //    {
    //        return;
    //    }

    //    builder.TryAddHealthCheck(
    //        healthCheckName,
    //        healthCheck => healthCheck.AddCouchbase(
    //            settings.ConnectionString,
    //            healthCheckName,
    //            null,
    //            null,
    //            settings.HealthCheckTimeout > 0 ? TimeSpan.FromMilliseconds(settings.HealthCheckTimeout.Value) : null));
    //}

    private static CouchbaseClient CreateCouchbaseClient(
        this IServiceProvider serviceProvider,
        string connectionName,
        string configurationSectionName,
        CouchbaseSettings couchbaseSettings,
        Action<ClusterOptions>? configureClusterSettings)
    {
        couchbaseSettings.ValidateSettings(connectionName, configurationSectionName);

        var clusterOptions = new ClusterOptions
        {
            ConnectionString = couchbaseSettings.ConnectionString
        };

        if (!couchbaseSettings.DisableTracing)
        {
            clusterOptions.WithTracing(new Couchbase.Core.Diagnostics.Tracing.TracingOptions { Enabled = true });
        }

        configureClusterSettings?.Invoke(clusterOptions);

        clusterOptions.Logging = serviceProvider.GetService<ILoggerFactory>();

        return new(clusterOptions);
    }

    private static CouchbaseSettings GetCouchbaseSettings(
           this IHostApplicationBuilder builder,
           string connectionName,
           string configurationSectionName,
           Action<CouchbaseSettings>? configureSettings)
    {
        var settings = new CouchbaseSettings();

        builder.Configuration
            .GetSection(configurationSectionName)
            .Bind(settings);

        if (builder.Configuration.GetConnectionString(connectionName) is string connectionString)
        {
            settings.ConnectionString = connectionString;
        }

        configureSettings?.Invoke(settings);

        return settings;
    }

    private static void ValidateSettings(
        this CouchbaseSettings settings,
        string connectionName,
        string configurationSectionName)
    {
        ConnectionStringValidation.ValidateConnectionString(settings.ConnectionString, connectionName, configurationSectionName);
    }
}
