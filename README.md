# ASP.NET Identity Permission Extension

This project aims to provide an extension to adding __Permission-based__ authorization capability to the __Microsoft Identity 2__ Membership System.

## DESCRIPTION

The ASP.NET Identity system is designed to replace the previous ASP.NET Membership and Simple Membership systems. It includes profile support, OAuth integration, works with OWIN, and is included with the ASP.NET templates. Despite a lot of flexibility to change the authentication workflow and adding custom behaviour, it would be nice to have an encapsulated and completely ready-to-use solution for Permission-based authentication. Using this extension you can define a `Permission` that many Roles can access to and stored in the `RolePermission` object. So there is a many-to-many relation between the `Permission` and `Role` objects and also database tables. 

### Solution Hierarchy:

This is a C#.NET solution-project, and it contains two subprojects:

 1. [IdentityPermission](IdentityPermission): A class library project which contains the Identity Permission Extension implementations.
 2. [Example](Example): An ASP.NET MVC 5 sample project which contains a web project that using the extension.

#### A Note to Contributors:
If you wish to contribute to this project, then please make sure you check out the [Contribution Guidelines](CONTRIBUTING.md) first.



## Getting Start
### Installation
For the moment you should build the class library project and add the made dll to references of the main project. We'll make a nuget package as soon as possible.
### Configuration
##### __1-__
Make the necessary models using Identity models and permission extension models as you can see in the [Examples Identity Model](Example/Models/IdentityModels.cs)

__Note:__ Make sure you used the `IdentityPermissionExtension.IdentityRole` instead of `Microsoft.AspNet.Identity.EntityFramework.IdentityRole` throughout the code.

##### __2-__ 
Go to the `OWIN Startup` class and add the `UsePermissionManager` middleware to the application pipeline in the Configuration method like below:
```csharp
public partial class Startup
{
    public void Configuration(IAppBuilder app)
    {
        app.UsePermissionManager(new PermissionManager());
    }
}
```
__Note:__ Pass an instance of your `PermissionManager` class to the middleware.

##### __3-__ 
Create a DbContext derived class from the `IdentityPermissionExtension.IdentityDbContext` like below:
```csharp
public class IdentityContext : IdentityDbContext<IdentityUser, IdentityRole, int, IdentityUserLogin, 
        IdentityUserRole, IdentityPermission, IdentityRolePermission, IdentityUserClaim>
{
    public static IdentityContext Create()
    {
        return new IdentityContext();
    }
}
```

##### __4-__ 
Use your classes in the right place throughout the code. For instance in the `App_Start/Startup.Auth.cs` like below:
```csharp
public partial class Startup
{
    public void ConfigureAuth(IAppBuilder app)
    {
        // Configure the db context, user manager and signin manager
        app.CreatePerOwinContext(IdentityContext.Create);
        app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
        app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
    }
}
```
__Note:__ You should rewite your related Identity components with your classes.

### Usage
There is two options to do authorization using the Permission Extension and `AuthorizePermission` method:

__1-__ As an attribute of Controllers or Actions:
```csharp
//
// GET: /Manage/Index
[AuthorizePermission(Name = "Show_Management", Description = "Show the Management Page.")]
public async Task<ActionResult> Index(ManageMessageId? message)
{
	//...
}
```
__2-__ As an extension method on the current HttpContext(HttpContextBase):
```csharp
//
// GET: /Manage/Users
public async Task<ActionResult> Users()
{
    if (await HttpContext.AuthorizePermission(name: "AllUsers_Management", description: "Edit all of the users information."))
    {
        return View(db.GetAllUsers());
    }
    else if (await HttpContext.AuthorizePermission(name: "UnConfirmedUsers_Management", description: "Edit unconfirmed users information."))
    {
        return View(db.GetUnConfirmedUsers());
    }
    else
    {
        return View(new List<User>());
    }
}
```

## CONTRIBUTORS
 * [Armin Khodaei](https://github.com/arminkhodaei)

## LICENSE
This project is licensed under the [MIT License](LICENSE).
