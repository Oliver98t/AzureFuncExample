using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class HttpTrigger2
{
    private readonly ILogger<HttpTrigger2> _logger;

    public HttpTrigger2(ILogger<HttpTrigger2> logger)
    {
        _logger = logger;
    }

    [Function("GetUserInfo")]
    public async Task<IActionResult> GetUserInfo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{id}")] HttpRequest req,
        string id)
    {
        _logger.LogInformation($"Getting user info for ID: {id}");

        // Simulate some async work
        await Task.Delay(100);

        var userInfo = new
        {
            Id = id,
            Name = $"User {id}",
            Email = $"user{id}@example.com",
            CreatedAt = DateTime.UtcNow
        };

        return new OkObjectResult(userInfo);
    }

    [Function("CreateUser")]
    public async Task<IActionResult> CreateUser(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")] HttpRequest req)
    {
        _logger.LogInformation("Creating new user");

        try
        {
            // Read the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            // In a real app, you'd deserialize and validate the JSON
            // var userData = JsonSerializer.Deserialize<UserData>(requestBody);

            var newUser = new
            {
                Id = Guid.NewGuid().ToString(),
                Message = "User created successfully",
                Data = requestBody,
                CreatedAt = DateTime.UtcNow
            };

            return new CreatedResult($"/api/users/{newUser.Id}", newUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return new BadRequestObjectResult("Invalid request data");
        }
    }
}