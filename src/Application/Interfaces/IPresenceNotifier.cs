namespace TaskManager.Application.Interfaces;

public interface IPresenceNotifier
{
    Task NotifyUserOnline(Guid userId, string userName, DateTime loginAt);
    Task NotifyUserOffline(Guid userId, string userName);
}