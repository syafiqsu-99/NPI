using NPI.Server.Services;
using System.Security.Claims;

namespace NPI.Server.Helpers
{
    public static class RbacHelper
    {
        public static bool IsAdminOrManager(ClaimsPrincipal user)
            => GetSystemRole(user) is SystemRoles.Admin or SystemRoles.Manager;

        public static bool IsAdminOrManager(string userRole)
            => string.Equals(userRole, SystemRoles.Admin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(userRole, SystemRoles.Manager, StringComparison.OrdinalIgnoreCase);

        public static int GetUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        public static string GetSystemRole(ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

        public static int GetDepartmentId(ClaimsPrincipal user)
            => int.TryParse(user.FindFirst("DepartmentId")?.Value, out var id) ? id : 0;

        public static async Task<bool> HasProjectAccess(
            ClaimsPrincipal user,
            int projectId,
            string minimumProjectRole,
            IProjectRoleService projectRoleService)
        {
            var systemRole = GetSystemRole(user);
            if (systemRole is "Admin" or "Manager")
                return true;

            var userId = GetUserId(user);
            return await projectRoleService.HasProjectRoleAsync(projectId, userId, minimumProjectRole);
        }

        public static string GetDepartmentName(ClaimsPrincipal user)
        => user.FindFirst("DepartmentName")?.Value ?? string.Empty;

        public static bool IsSalesUser(ClaimsPrincipal user)
            => string.Equals(GetDepartmentName(user), "Sales", StringComparison.OrdinalIgnoreCase);

        public static bool CanCreateEnquiry(ClaimsPrincipal user)
        {
            var role = GetSystemRole(user);
            return role is "Admin" or "Manager" || IsSalesUser(user);
        }

        public static bool CanDeleteEnquiry(ClaimsPrincipal user, string status, int createdBy)
        {
            var role = GetSystemRole(user);
            if (role is "Admin" or "Manager") return true;
            return IsSalesUser(user) && status == EnquiryStatus.Draft && createdBy == GetUserId(user);
        }
    }
}
