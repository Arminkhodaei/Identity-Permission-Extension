using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Microsoft.AspNet.Identity;

namespace IdentityPermissionExtension
{
    /// <summary>
    /// Permission manager using a IPermissionStore instance to intraction with database.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TPermission"></typeparam>
    /// <typeparam name="TRolePermission"></typeparam>
    public class PermissionManager<TKey, TPermission, TRolePermission> : IPermissionManager
        where TPermission : class, IPermission<TKey, TRolePermission>
        where TRolePermission : IdentityRolePermission<TKey>
        where TKey : IEquatable<TKey>
    {
        protected internal IPermissionStore<TKey, TPermission> Store { get; set; }

        public PermissionManager(IPermissionStore<TKey, TPermission> permissionStore)
        {
            this.Store = permissionStore;
        }

        public virtual async Task InitialConfiguration()
        {
            await this.Store.InitialConfiguration();
        }

        public virtual async Task<IList<string>> GetRolesAsync(int userId, ClaimsPrincipal user = null)
        {
            return await this.Store.GetRolesAsync(userId, user);
        }

        public virtual async Task<TPermission> FindPermissionAsync(string name, long origin, bool isGlobal)
        {
            return await this.Store.FindPermissionAsync(name, origin, isGlobal);
        }

        public virtual async Task<TPermission> CreatePermissionAsync(string name, long origin, string description = null,
            bool isGlobal = false, HttpContextBase httpContext = null)
        {
            return await this.Store.CreatePermissionAsync(name, origin, description, isGlobal, httpContext);
        }

        public virtual async Task DeletePermissionAsync(TKey id)
        {
            await this.Store.DeletePermissionAsync(id);
        }

        /// <summary>
        /// Check weather the permission assigned to the list of roles or not.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="roles"></param>
        /// <param name="description"></param>
        /// <param name="isGlobal"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public virtual async Task<bool> CheckPermissionAsync(string name, IList<string> roles, string description = null,
            bool isGlobal = false, HttpContextBase httpContext = null)
        {
            var origin = GetOrigin(httpContext);
            var permission = await FindPermissionAsync(name, origin, isGlobal) ??
                             await CreatePermissionAsync(name, origin, description, isGlobal, httpContext);
            foreach (var role in await this.Store.GetRolesAsync(permission))
            {
                if (roles.Contains(role))
                    return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Using the RouteData to find the permission origin.
        /// </summary>
        /// <returns>Return a long hash number. if is zero, the permission defined out of the MVC structure scope.</returns>
        public virtual long GetOrigin(HttpContextBase httpContext = null)
        {
            var mvcOrigin = "";
            if (httpContext != null)
            {
                mvcOrigin += RouteTable.Routes?.GetRouteData(httpContext)?.Values["area"]?.ToString();
                mvcOrigin += RouteTable.Routes?.GetRouteData(httpContext)?.Values["controller"]?.ToString();
                mvcOrigin += RouteTable.Routes?.GetRouteData(httpContext)?.Values["action"]?.ToString();
            }

            if (string.IsNullOrEmpty(mvcOrigin))
                return 0;

            return new ByteEncoder(mvcOrigin).ToLong();
        }

        public virtual async Task<bool> AuthorizePermissionAsync(string name, string description = null,
            bool isGlobal = false)
        {
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
                return await Task.FromResult(false);

            var stringUserId = Thread.CurrentPrincipal?.Identity?.GetUserId();
            if (string.IsNullOrEmpty(stringUserId))
                throw new ArgumentNullException(nameof(stringUserId));

            var roles =
                await
                    this.GetRolesAsync(int.Parse(stringUserId), (ClaimsPrincipal)Thread.CurrentPrincipal);

            return await this.CheckPermissionAsync(name, roles, description, isGlobal);
        }

        public virtual async Task<bool> AuthorizePermissionAsync(HttpContextBase httpContext, string name,
            string description = null, bool isGlobal = false)
        {
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
                return await Task.FromResult(false);

            var stringUserId = Thread.CurrentPrincipal?.Identity?.GetUserId();
            if (string.IsNullOrEmpty(stringUserId))
                throw new ArgumentNullException(nameof(stringUserId));

            var roles =
                await
                    this.GetRolesAsync(int.Parse(stringUserId), (ClaimsPrincipal)Thread.CurrentPrincipal);

            return await this.CheckPermissionAsync(name, roles, description, isGlobal, httpContext);
        }

        public void Dispose()
        {
            this.Store = null;
        }
    }
}