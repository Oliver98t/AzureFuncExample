using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;

namespace Company.Function;

public class ItemFunctions
{
    private readonly ILogger<UserFunctions> _logger;
    private readonly TableServiceClient _tableServiceClient;

    public ItemFunctions(ILogger<UserFunctions> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;
    }

    [Function("GetItem")]
    public async Task<IActionResult> GetItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "item/{id}")] HttpRequest req,
        string id)
    {
        _logger.LogInformation($"Getting user info for ID: {id}");
        var tableClient = _tableServiceClient.GetTableClient("ItemTable");
        var items = tableClient.QueryAsync<TableEntity>(e => e.PartitionKey == "Books");

        await foreach(var item in items)
        {
            _logger.LogInformation($"Found: {item.RowKey}, Message: {item["Message"]}");
        }
        return new OkResult();
    }

    [Function("CreateItem")]
    public async Task<IActionResult> CreateItem(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "item")] HttpRequest req)
    {
        _logger.LogInformation("Creating new user");

        try
        {
            // Read the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            // In a real app, you'd deserialize and validate the JSON
            // var userData = JsonSerializer.Deserialize<UserData>(requestBody);

            Console.WriteLine(requestBody);
            // Get or create table
            var tableClient = _tableServiceClient.GetTableClient("ItemTable");
            await tableClient.CreateIfNotExistsAsync();
            
            // Add entity
            var entity = new TableEntity("Books", Guid.NewGuid().ToString())
            {
                { "Message", "Welcome to Azure Functions!" },
                { "Timestamp", DateTime.UtcNow }
            };
            await tableClient.AddEntityAsync(entity);

            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return new BadRequestObjectResult("Invalid request data");
        }
    }
}