using ClinicManagementSystem.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace ClinicManagementSystem.Permissions;

public class ClinicManagementSystemPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ClinicManagementSystemPermissions.GroupName);

        var studentsPermission = myGroup.AddPermission(
            ClinicManagementSystemPermissions.Students.Default,
            L("Permission:Students"));

        studentsPermission.AddChild(
            ClinicManagementSystemPermissions.Students.Create,
            L("Permission:Students.Create"));

        studentsPermission.AddChild(
            ClinicManagementSystemPermissions.Students.Edit,
            L("Permission:Students.Edit"));

        studentsPermission.AddChild(
            ClinicManagementSystemPermissions.Students.Delete,
            L("Permission:Students.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ClinicManagementSystemResource>(name);
    }
}
