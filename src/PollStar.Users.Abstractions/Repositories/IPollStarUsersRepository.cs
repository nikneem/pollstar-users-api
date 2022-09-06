using PollStar.Users.Abstractions.DataTransferObjects;

namespace PollStar.Users.Abstractions.Repositories;

public interface IPollStarUsersRepository
{
    Task<bool> CreateAsync(Guid userId);
    Task<UserDto?> GetAsync(Guid userId);
}