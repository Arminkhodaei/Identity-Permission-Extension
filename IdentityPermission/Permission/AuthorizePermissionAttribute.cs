using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IdentityPermissionExtension
{
    /// <summary>
    /// Permission-based authorization attribute.
    /// </summary>
    public class AuthorizePermissionAttribute : AuthorizeAttribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsGlobal { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return base.AuthorizeCore(httpContext)
                   && Task.Run(() => httpContext.AuthorizePermission(Name, Description, IsGlobal)).Result;
        }
    }
}