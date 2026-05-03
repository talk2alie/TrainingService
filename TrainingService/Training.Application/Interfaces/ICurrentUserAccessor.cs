namespace Training.Application.Interfaces;

public interface ICurrentUserAccessor
{
    Guid GetRequiredUserId();
}
