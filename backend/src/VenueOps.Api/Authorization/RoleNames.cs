namespace VenueOps.Api.Authorization;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Staff = "Staff";
    public const string Demo = "Demo";

    public const string Operations = $"{Admin},{Manager}";
    public const string AllRoles = $"{Admin},{Manager},{Staff},{Demo}";
    public const string AssignableStaffReaders = $"{Admin},{Manager},{Demo}";
}
