using Example.Models;
using IdentityPermissionExtension;

namespace Example.Identity
{
    /// <summary>
    /// A DbContext class that inherited from IdentityDbContext of Permission extension.
    /// </summary>
    public class IdentityContext
        :
            IdentityDbContext
            <IdentityUser, IdentityRole, int, IdentityUserLogin, IdentityUserRole, IdentityPermission,
                IdentityRolePermission, IdentityUserClaim>
    {
        /// <summary>
        /// A static method which create a new instance of this DbContext
        /// </summary>
        /// <returns></returns>
        public static IdentityContext Create()
        {
            return new IdentityContext();
        }
    }
}