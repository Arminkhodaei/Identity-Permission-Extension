using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.Identity;

namespace IdentityPermissionExtension
{
    /// <summary>
    ///     EntityFramework based permission store implementation
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TUserLogin"></typeparam>
    /// <typeparam name="TRolePermission"></typeparam>
    /// <typeparam name="TUserClaim"></typeparam>
    /// <typeparam name="TUserRole"></typeparam>
    /// <typeparam name="TPermission"></typeparam>
    /// <typeparam name="TUserStore"></typeparam>
    public class PermissionStore<TKey, TRole, TUser, TUserStore, TUserLogin, TRolePermission, TUserClaim, TUserRole, TPermission>
        : IPermissionStore<TKey, TPermission>
        where TUserStore : UserStore<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim>
        where TUser : IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TPermission : IdentityPermission<TKey, TRolePermission>, new()
        where TRolePermission : IdentityRolePermission<TKey>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
        where TRole : IdentityRole<TKey, TUserRole, TRolePermission>, new()
        where TKey : IEquatable<TKey>
    {
        private readonly IDbSet<TRole> _roles;
        private readonly IDbSet<TPermission> _permissions;
        private readonly IDbSet<TUser> _user;

        /// <summary>Context for the store</summary>
        public IdentityDbContext<TUser, TRole, TKey, TUserLogin, TUserRole, TPermission, TRolePermission, TUserClaim>
            Context
        { get; private set; }

        /// <summary>
        ///     Constructor which takes a db context and wires up the stores with default instances using the context
        /// </summary>
        /// <param name="context"></param>
        public PermissionStore(
            IdentityDbContext<TUser, TRole, TKey, TUserLogin, TUserRole, TPermission, TRolePermission, TUserClaim>
                context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            this.Context = context;
            this._roles = (IDbSet<TRole>)this.Context.Set<TRole>();
            this._permissions = (IDbSet<TPermission>)this.Context.Set<TPermission>();
            this._user = (IDbSet<TUser>)this.Context.Set<TUser>();
        }

        /// <summary>
        /// Invoke initial configuration of permission extension. For instance create an Admin user and at least two main roles.
        /// </summary>
        /// <returns></returns>
        public virtual async Task InitialConfiguration()
        {
            if (!this._roles.Any(r => r.Name == nameof(Roles.Administrator)))
                this._roles.Add(new TRole { Name = nameof(Roles.Administrator), Title = "Super Administrator" });
            if (!this._roles.Any(r => r.Name == nameof(Roles.User)))
                this._roles.Add(new TRole { Name = nameof(Roles.User), Title = "General User" });

            this.Context.SaveChanges();

            var adminRole = this._roles.First(x => x.Name == nameof(Roles.Administrator));
            if (!this._user.Any(u => u.Roles.Any(r => r.RoleId.Equals(adminRole.Id))))
            {
                var userStore = Activator.CreateInstance(typeof(TUserStore), new object[] { Context });
                var userManager = Activator.CreateInstance(typeof(UserManager<TUser, TKey>), new object[] { userStore });
                var admin = new TUser
                {
                    UserName = "Admin",
                    Email = "Admin@here.com"
                };
                await ((UserManager<TUser, TKey>)userManager).CreateAsync(admin, "Admin@123");
                await ((UserManager<TUser, TKey>)userManager).AddToRoleAsync(admin.Id, nameof(Roles.Administrator));
            }
        }

        /// <summary>
        /// Get the roles that is assigned to the user ClaimPrincipal object.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual async Task<IList<string>> GetRolesAsync(int userId, ClaimsPrincipal user = null)
        {
            if (userId == 0 && user == null)
                throw new ArgumentNullException(nameof(userId));
            if (user == null || !user.Claims.Any())
                return await this._roles.Where(x => x.Users.Any(u => userId.Equals(u.UserId)))
                    .Select(r => r.Name).ToListAsync();
            //if else there is a user claims
            return
                await
                    Task.Run(() => user.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList());
        }

        /// <summary>
        /// Get the roles that is assigned to the permission object.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public virtual async Task<IList<string>> GetRolesAsync(TPermission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));
            if (default(TKey).Equals(permission.Id))
                throw new ArgumentNullException(nameof(permission.Id));
            return
                await
                    this._roles.Where(x => x.Permissions.Any(p => p.PermissionId.Equals(permission.Id)))
                        .Select(x => x.Name)
                        .ToListAsync();
        }

        public virtual async Task<TPermission> FindPermissionAsync(string name, long origin, bool isGlobal)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (isGlobal || origin == 0)
            {
                return await Task
                    .FromResult((TPermission)this._permissions.FirstOrDefault(x => x.Name == name && x.IsGlobal));
            }

            return await Task
                .FromResult((TPermission)this._permissions.FirstOrDefault(x => x.Name == name && x.Origin == origin));
        }

        /// <summary>
        /// Create a new permission.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="origin"></param>
        /// <param name="description"></param>
        /// <param name="isGlobal"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task<TPermission> CreatePermissionAsync(string name, long origin, string description = null,
            bool isGlobal = false, HttpContextBase httpContext = null)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            string area = null, controller = null, action = null;
            TPermission permission = await FindPermissionAsync(name, origin, isGlobal);

            if (permission?.Name.ToLower() == name)
                throw new ArgumentException("The " + nameof(name) + " argument must be unique.");

            if (httpContext != null)
            {
                area = RouteTable.Routes?.GetRouteData(httpContext)?.Values[nameof(area)]?.ToString();
                controller = RouteTable.Routes?.GetRouteData(httpContext)?.Values[nameof(controller)]?.ToString();
                action = RouteTable.Routes?.GetRouteData(httpContext)?.Values[nameof(action)]?.ToString();
            }

            var newPerm = new TPermission();
            //create a new permission
            if (permission == null)
            {
                newPerm = new TPermission()
                {
                    Name = name,
                    Origin = origin,
                    AreaName = area,
                    ControllerName = controller,
                    ActionName = action,
                    Description = description,
                    IsGlobal = origin == 0 || isGlobal
                };
                this._permissions.Add(newPerm);
                this.Context.SaveChanges();

                //Add the administrator role to the permission
                await AddInitialRolesAsync(newPerm);

                this.Context.SaveChanges();
            }
            //the same permission existed
            else
            {
                int convert;
                var last = permission.Name.Split('_').Last();
                int.TryParse(last, out convert);
                if (convert != 0)
                {
                    convert++;
                    newPerm.Name = permission.Name
                        .Split('_')
                        .Except(new[] { last })
                        .Concat(new[] { convert.ToString() })
                        .Aggregate((x, y) => x + y);
                }
                else
                {
                    convert = 1;
                    newPerm.Name = permission.Name
                        .Insert(permission.Name.Length - 1, convert.ToString());
                }
                newPerm.Origin = origin;
                newPerm.AreaName = area;
                newPerm.ControllerName = controller;
                newPerm.ActionName = action;
                newPerm.Description = description;
                newPerm.IsGlobal = origin == 0 || isGlobal;

                this._permissions.Add(newPerm);
                this.Context.SaveChanges();

                //Add the administrator role to the permission
                await AddInitialRolesAsync(newPerm);

                this.Context.SaveChanges();
            }

            return await Task.FromResult(newPerm);
        }

        public virtual async Task DeletePermissionAsync(TKey id)
        {
            var key = default(TKey);
            if (key != null && key.Equals(id))
                throw new ArgumentNullException(nameof(id));
            var permission = this.Context.Permissions.Find(id);
            if (permission == null)
                throw new Exception("Permission not found.");
            this._permissions.Remove(permission);
            await this.Context.SaveChangesAsync();
        }

        /// <summary>
        /// Add the administrator role to the specified permission.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        private async Task AddInitialRolesAsync(TPermission permission)
        {
            if (permission.Id.Equals(default(TKey)))
                throw new ArgumentNullException(nameof(permission.Id));

            await Task.Run(() =>
            {

                var adminRole = new TRolePermission()
                {
                    RoleId = this._roles.First(x => x.Name == nameof(Roles.Administrator)).Id,
                    PermissionId = permission.Id
                };

                if (permission.Roles != null)
                    permission.Roles.Add(adminRole);
                else
                {
                    permission.Roles = new List<TRolePermission>()
                    {
                        adminRole
                    };
                }
            });
        }
    }
}