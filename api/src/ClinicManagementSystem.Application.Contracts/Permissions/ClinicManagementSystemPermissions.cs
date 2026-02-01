namespace ClinicManagementSystem.Permissions;

public static class ClinicManagementSystemPermissions
{
    public const string GroupName = "ClinicManagementSystem";

    public static class Students
    {
        public const string Default = GroupName + ".Students";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
}
