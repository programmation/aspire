// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire;
using Aspire.Couchbase;

[assembly: ConfigurationSchema("Aspire:Couchbase", typeof(CouchbaseSettings))]

[assembly: LoggingCategories(
    "Couchbase",
    "Couchbase.Connection",
    "Couchbase.Internal")]
