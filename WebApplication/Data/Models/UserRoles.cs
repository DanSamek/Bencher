namespace WebApplication.Data.Models;

/// <summary>
/// For app security it's required to have some simple roles.
/// </summary>
[Flags]
public enum UserRole
{
    NotVerified = 0x1,
    Verified = 0x2,
    Admin = 0x4,
}