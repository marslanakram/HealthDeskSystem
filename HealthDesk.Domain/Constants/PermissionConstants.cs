namespace HealthDesk.Domain.Constants;

public static class PermissionConstants
{
    // ── Patient Permissions ───────────────────────────────────────
    public static class Patients
    {
        public const string View = "Permissions.Patients.View";
        public const string Create = "Permissions.Patients.Create";
        public const string Edit = "Permissions.Patients.Edit";
        public const string Delete = "Permissions.Patients.Delete";
    }

    // ── Doctor Permissions ────────────────────────────────────────
    public static class Doctors
    {
        public const string View = "Permissions.Doctors.View";
        public const string Create = "Permissions.Doctors.Create";
        public const string Edit = "Permissions.Doctors.Edit";
        public const string Delete = "Permissions.Doctors.Delete";
    }

    // ── Appointment Permissions ───────────────────────────────────
    public static class Appointments
    {
        public const string View = "Permissions.Appointments.View";
        public const string Create = "Permissions.Appointments.Create";
        public const string Edit = "Permissions.Appointments.Edit";
        public const string Delete = "Permissions.Appointments.Delete";
        public const string UpdateStatus = "Permissions.Appointments.UpdateStatus";
    }

    // ── Prescription Permissions ──────────────────────────────────
    public static class Prescriptions
    {
        public const string View = "Permissions.Prescriptions.View";
        public const string Create = "Permissions.Prescriptions.Create";
        public const string Delete = "Permissions.Prescriptions.Delete";
    }

    // ── Department Permissions ────────────────────────────────────
    public static class Departments
    {
        public const string View = "Permissions.Departments.View";
        public const string Create = "Permissions.Departments.Create";
        public const string Edit = "Permissions.Departments.Edit";
        public const string Delete = "Permissions.Departments.Delete";
    }

    // ── User Management Permissions ───────────────────────────────
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
    }

    // ── Role Management Permissions ───────────────────────────────
    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
        public const string Delete = "Permissions.Roles.Delete";
        public const string ManagePermissions = "Permissions.Roles.ManagePermissions";
    }

    // ── Helper: Get All Permissions ───────────────────────────────
    public static List<string> GetAllPermissions()
    {
        var permissions = new List<string>();

        // Use reflection to get all const string fields
        var types = typeof(PermissionConstants).GetNestedTypes();
        foreach (var type in types)
        {
            var fields = type.GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    var value = field.GetRawConstantValue()?.ToString();
                    if (!string.IsNullOrEmpty(value))
                        permissions.Add(value);
                }
            }
        }

        return permissions;
    }

    // ── Helper: Get Permissions by Category ───────────────────────
    public static Dictionary<string, List<string>> GetPermissionsByCategory()
    {
        var result = new Dictionary<string, List<string>>();

        var types = typeof(PermissionConstants).GetNestedTypes();
        foreach (var type in types)
        {
            var category = type.Name;
            var perms = new List<string>();

            var fields = type.GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    var value = field.GetRawConstantValue()?.ToString();
                    if (!string.IsNullOrEmpty(value))
                        perms.Add(value);
                }
            }

            result[category] = perms;
        }

        return result;
    }
}
