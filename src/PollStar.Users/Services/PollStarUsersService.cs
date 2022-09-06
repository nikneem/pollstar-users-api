using Microsoft.Extensions.Logging;
using PollStar.Users.Abstractions.DataTransferObjects;
using PollStar.Users.Abstractions.Repositories;
using PollStar.Users.Abstractions.Services;
using PollStar.Users.ErrorCodes;
using PollStar.Users.Exceptions;

namespace PollStar.Users.Services;

public class PollStarUsersService : IPollStarUsersService
{
    private readonly IPollStarUsersRepository _repository;
    private readonly ILogger<PollStarUsersService> _logger;

    public async Task<UserDto> Create()
    {
        var userId = Guid.NewGuid();
        _logger.LogInformation("New user GUID {userId} generated", userId);
        try
        {
            if (await _repository.CreateAsync(userId))
            {
                _logger.LogInformation("Created a new user entry with ID {userId}", userId);
                return new UserDto
                {
                    UserId = userId
                };
            }
        }
        catch (Exception ex)
        {
            throw new PollStarUsersException(PollStarUsersErrorCode.FailedToCreateUser, "An exception occured while trying to create a new user", ex);
        }

        _logger.LogInformation("Failed to create User ID, user will receive an exception");
        throw new PollStarUsersException(PollStarUsersErrorCode.FailedToCreateUser, "An unknown error occured while trying to create a new user" );
    }

    public async Task<UserDto> Restore(Guid userId)
    {
        var user = await _repository.GetAsync(userId) ?? await Create();
        return user;
    }


    public PollStarUsersService(
        IPollStarUsersRepository repository,
        ILogger<PollStarUsersService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

}