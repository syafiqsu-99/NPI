using NPI.Server.Services;
using System.Security.Claims;

namespace NPI.Server.Helpers
{
    public static class RbacHelper
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        public static string GetSystemRole(ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

        /// <summary>
        /// Returns true if user has AT LEAST the required project-level role.
        /// System Admins and Managers always pass.
        /// </summary>
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
    }
}
