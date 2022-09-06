using PollStar.Core.ErrorCodes;

namespace PollStar.Users.ErrorCodes;

public abstract class PollStarUsersErrorCode : PollStarErrorCode
{
    public static readonly PollStarUsersErrorCode FailedToCreateUser = new FailedToCreateUserErrorCode();
    public override string ErrorNamespace => "Errors.Users";
}
