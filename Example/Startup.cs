using Example.Identity;
using Example.Models;
using Microsoft.Owin;
using Owin;
using IdentityPermissionExtension;

[assembly: OwinStartupAttribute(typeof(Example.Startup))]
namespace Example
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            //InitialConfiguration and define the Permission manager of identity permission extension.
            app.UsePermissionManager(new IdentityPermissionManager(new IdentityPermissionStore(new IdentityContext())));
        }
    }
}
