using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Example.Identity;

namespace Example.Models
{
    /// <summary>
    /// Identity models that haven't changed in presence of permission extension.
    /// </summary>
    #region Original Model

    public class IdentityUser : IdentityUser<int, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(IdentityUserManager manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class IdentityUserLogin : IdentityUserLogin<int>
    {

    }

    public class IdentityUserRole : IdentityUserRole<int>
    {

    }

    public class IdentityUserClaim : IdentityUserClaim<int>
    {

    }
    #endregion

    /// <summary>
    /// New models or identity models that have changed in presence of permission extension.
    /// </summary>
    #region Permission Model
    public class IdentityRolePermission : IdentityPermissionExtension.IdentityRolePermission<int>
    {

    }

    public class IdentityPermission : IdentityPermissionExtension.IdentityPermission<int, IdentityRolePermission>
    {

    }

    /// <summary>
    /// The new IdentityRole should use insted of the original one.
    /// </summary>
    public class IdentityRole : IdentityPermissionExtension.IdentityRole<int, IdentityUserRole, IdentityRolePermission>
    {

    }
    #endregion

    /// <summary>
    /// Note: use the permission extension DbContext in the constructor of the Stores.
    /// </summary>
    #region Stores
    public class IdentityUserStore : UserStore<IdentityUser, IdentityRole, int, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        public IdentityUserStore(IdentityContext context) : base(context)
        {
        }
    }

    public class IdentityRoleStore : RoleStore<IdentityRole, int, IdentityUserRole>
    {
        public IdentityRoleStore(IdentityContext context) : base(context)
        {
        }
    }

    /// <summary>
    /// PermissionStore Object
    /// </summary>
    public class IdentityPermissionStore : IdentityPermissionExtension.PermissionStore<int, IdentityRole, IdentityUser, IdentityUserStore, IdentityUserLogin,
        IdentityRolePermission, IdentityUserClaim, IdentityUserRole, IdentityPermission>
    {
        public IdentityPermissionStore(IdentityContext context) : base(context)
        {
        }
    }
    #endregion

    #region Managers
    public class IdentityUserManager : UserManager<IdentityUser, int>
    {
        public IdentityUserManager(IdentityUserStore store) : base(store)
        {
        }
    }

    public class IdentityRoleManager : RoleManager<IdentityRole, int>
    {
        public IdentityRoleManager(IdentityRoleStore store) : base(store)
        {
        }
    }

    /// <summary>
    /// PermissionManager Object
    /// </summary>
    public class IdentityPermissionManager : IdentityPermissionExtension.PermissionManager<int, IdentityPermission, IdentityRolePermission>
    {
        public IdentityPermissionManager(IdentityPermissionStore store) : base(store)
        {
        }
    }
    #endregion
}