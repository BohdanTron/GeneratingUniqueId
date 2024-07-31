using FluentAssertions;
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


// Tests
// Arrange
try
{
    var tasks = Enumerable.Range(1, 100)
        .Select(_ => container.PatchItemAsync<Sequence>("sample", new PartitionKey("sample"), operations))
        .ToList();

    // Act
    var result = await Task.WhenAll(tasks);

    // Assert
    var totalCount = result.Length;
    var distinctCount = result.Distinct().Count();

    totalCount.Should().Be(distinctCount);

    var ordered = result.Select(item => item.Resource.Value).OrderBy(number => number).ToList();
    for (var i = 0; i < ordered.Count; i++)
    {
        ordered[i].Should().Be(i + 2); // Need to use "i + 2" since we already inserted the first value into container
    }
}
finally
{
    await database.Database.DeleteAsync();
}

internal class Sequence
{
    [JsonProperty("id")]
    public string Id { get; set; } = default!;

    public decimal Value { get; set; }
}