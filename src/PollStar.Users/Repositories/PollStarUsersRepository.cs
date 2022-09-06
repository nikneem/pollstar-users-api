using Azure;
using Azure.Data.Tables;
using HexMaster.RedisCache.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PollStar.Core;
using PollStar.Core.Configuration;
using PollStar.Users.Abstractions.DataTransferObjects;
using PollStar.Users.Abstractions.Repositories;
using PollStar.Users.Repositories.Entities;
using Constants = HexMaster.RedisCache.Abstractions.Constants;

namespace PollStar.Users.Repositories;

public class PollStarUsersRepository : IPollStarUsersRepository
{
    private readonly ILogger<PollStarUsersRepository> _logger;
    private TableClient _tableClient;
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
        _logger.LogInformation("Now storing user in users repository");
        var response = await _tableClient.AddEntityAsync(entity);
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
            var response = await _tableClient.GetEntityAsync<UserTableEntity>(PartitionKey, userId.ToString());
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch user {userId} from data store", userId);
        }
        return null;
    }

    public PollStarUsersRepository(
        IOptions<AzureConfiguration> options,
        ILogger<PollStarUsersRepository> logger)
    {
        _logger = logger;
        var accountName = options.Value.StorageAccount;
        var accountKey = options.Value.StorageKey;
        var storageUri = new Uri($"https://{accountName}.table.core.windows.net");
        _tableClient = new TableClient(
            storageUri,
            TableName,
            new TableSharedKeyCredential(accountName, accountKey));
    }

}