using Aspire.Couchbase;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddCouchbaseClient("couchbase");

var app = builder.Build();

app.MapGet("/", async (ICouchbaseClient couchbaseClient) =>
{
    //const string bucketName = "entries";

    return await Task.FromResult(new List<Entry>());
});

public class Entry
{
    public string? Id { get; set; }
}
