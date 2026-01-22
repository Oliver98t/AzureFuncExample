using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;

namespace Company.Function;

public class UserFunctions
{
    private readonly ILogger<UserFunctions> _logger;
    private readonly TableServiceClient _tableServiceClient;

    public UserFunctions(ILogger<UserFunctions> logger, TableServiceClient tableServiceClient)
    {
        _logger = logger;
        _tableServiceClient = tableServiceClient;
    }

    [Function("GetUser")]
    public async Task<IActionResult> GetUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user")] 
        HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        
        // Get or create table
        var tableClient = _tableServiceClient.GetTableClient("MyTable");
        await tableClient.CreateIfNotExistsAsync();
        
        // Add entity
        var entity = new TableEntity("PartitionKey", Guid.NewGuid().ToString())
        {
            { "Message", "Welcome to Azure Functions!" },
            { "Timestamp", DateTime.UtcNow }
        };
        await tableClient.AddEntityAsync(entity);
        
        return new OkObjectResult("Data saved to Table Storage!");
    }
}