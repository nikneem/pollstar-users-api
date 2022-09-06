namespace PollStar.Users.ErrorCodes;

public class FailedToCreateUserErrorCode : PollStarUsersErrorCode
{
    public override string Code => GetType().Name;
}