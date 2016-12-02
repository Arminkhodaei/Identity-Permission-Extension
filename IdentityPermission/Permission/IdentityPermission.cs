using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace IdentityPermissionExtension
{
    public class IdentityPermission<TKey, TRolePermission>
        : IPermission<TKey, TRolePermission>
        where TRolePermission : IdentityRolePermission<TKey>
    {
        public virtual TKey Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual long Origin { get; set; }
        public virtual string ControllerName { get; set; }
        public virtual string ActionName { get; set; }
        public virtual string AreaName { get; set; }

        /// <summary>
        /// Check whether the permission is accessible at the global level or local (using origin value)
        /// </summary>
        public virtual bool IsGlobal { get; set; }

        public virtual ICollection<TRolePermission> Roles { get; set; }
    }

    public class IdentityRolePermission<TKey> : IRolePermission<TKey>
    {
        public virtual TKey RoleId { get; set; }
        public virtual TKey PermissionId { get; set; }
    }

    public class IdentityRole<TKey, TUserRole, TRolePermission>
        : Microsoft.AspNet.Identity.EntityFramework.IdentityRole<TKey, TUserRole>,
            IIdentityRole<TKey, TRolePermission>
        where TRolePermission : IdentityRolePermission<TKey>
        where TUserRole : IdentityUserRole<TKey>
    {
        public string Title { get; set; }

        public virtual ICollection<TRolePermission> Permissions { get; set; }
    }
}