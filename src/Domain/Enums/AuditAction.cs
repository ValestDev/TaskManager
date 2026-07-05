namespace TaskManager.Domain.Enums;

public enum AuditAction
{
    Login,
    Logout,
    UserCreated,
    UserUpdated,
    UserDeactivated,
    UserReactivated,
    TaskCreated,
    TaskUpdated,
    TaskAssigned
}