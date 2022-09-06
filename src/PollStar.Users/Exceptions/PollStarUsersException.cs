using PollStar.Core.Exceptions;
using PollStar.Users.ErrorCodes;

namespace PollStar.Users.Exceptions;

public class PollStarUsersException : PollStarException
{
    public PollStarUsersException(PollStarUsersErrorCode errorCode, string message, Exception? ex = null) : base(errorCode, message, ex)
    {
    }
}