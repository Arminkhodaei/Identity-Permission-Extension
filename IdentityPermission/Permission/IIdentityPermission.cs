using System.Collections.Generic;
using Microsoft.AspNet.Identity;

namespace IdentityPermissionExtension
{
    public interface IPermission<out TKey, TRolePermission>
        where TRolePermission : IRolePermission<TKey>
    {
        TKey Id { get; }
        string Name { get; set; }
        long Origin { get; set; }
        bool IsGlobal { get; set; }
        ICollection<TRolePermission> Roles { get; set; }
    }
    public interface IRolePermission<TKey>
    {
        TKey RoleId { get; set; }
        TKey PermissionId { get; set; }
    }
    public interface IIdentityRole<out TKey, TRolePermission> : IRole<TKey>
    where TRolePermission : IdentityRolePermission<TKey>
    {
        ICollection<TRolePermission> Permissions { get; set; }
    }
}