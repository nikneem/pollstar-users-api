using Azure;
using Microsoft.Extensions.Logging;
using PollStar.Core.Factories;
using PollStar.Users.Abstractions.DataTransferObjects;
using PollStar.Users.Abstractions.Repositories;
using PollStar.Users.Repositories.Entities;

namespace PollStar.Users.Repositories;

public class PollStarUsersRepository : IPollStarUsersRepository
{
    private readonly IStorageTableClientFactory _tableClientFactory;
    private readonly ILogger<PollStarUsersRepository> _logger;
    private const string TableName = "users";
    private const string PartitionKey = "user";

    public async Task<bool> CreateAsync(Guid userId)
    {
        var entity = new UserTableEntity
        {
            PartitionKey = PartitionKey,
            RowKey = userId.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            ETag = ETag.All
        };
        var tableClient = _tableClientFactory.CreateClient(TableName);
        _logger.LogInformation("Now storing user in users repository");
        var response = await tableClient.AddEntityAsync(entity);
        return !response.IsError;
    }
    public async Task<UserDto?> GetAsync(Guid userId)
    {
        _logger.LogTrace("Fetching user from repository");
        var userEntity = await GetUserByUserIsAsync(userId);
        if (userEntity != null)
        {
            _logger.LogTrace("User found, returning appropriate information");
            return new UserDto
            {
                UserId = Guid.Parse(userEntity.RowKey)
            };
        }

        _logger.LogTrace("Oops, user not found, returning null reference");
        return null;
    }

    private async Task<UserTableEntity?> GetUserByUserIsAsync(Guid userId)
    {
        try
        {
            var tableClient = _tableClientFactory.CreateClient(TableName);
            var response = await tableClient.GetEntityAsync<UserTableEntity>(PartitionKey, userId.ToString());
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user {userId} from data store", userId);
        }
        return null;
    }

    public PollStarUsersRepository(
        IStorageTableClientFactory tableClientFactory,
        ILogger<PollStarUsersRepository> logger)
    {
        _tableClientFactory = tableClientFactory;
        _logger = logger;
    }

}