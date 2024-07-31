using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

// Database setup
var connectionString = "";
var cosmosClient = new CosmosClient(connectionString);

var database = await cosmosClient.CreateDatabaseIfNotExistsAsync("UniqueGeneratorDb");
var container = (await database.Database.CreateContainerIfNotExistsAsync("Sequence", "/id")).Container;

// Initial Data
using var responseMessage = await container.ReadItemStreamAsync("sample", new PartitionKey("sample"));
if (!responseMessage.IsSuccessStatusCode)
    await container.CreateItemAsync(new Sequence { Id = "sample", Value = 0 }, new PartitionKey("sample"));

// Generating unique number
var operations = new[]
{
    PatchOperation.Increment("/Value", 1)
};
var response = await container.PatchItemAsync<Sequence>("sample", new PartitionKey("sample"), operations);

Console.WriteLine($"Seq : {response.Resource.Value}, Consumed RU/s = {response.RequestCharge}");

internal class Sequence
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    public decimal Value { get; set; }
}