namespace Training.Application.Common.Interfaces;

public interface IUserContext
{
    Guid UserId { get; }
    string Role { get; }
    bool HasActiveSubscription { get; }
}
