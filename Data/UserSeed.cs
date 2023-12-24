using BlazorApp1.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.Security;

namespace BlazorApp1.Data {
  public static class UserSeed {

    public const string SuperAdmin = "SUPERADMIN";
    public const string Admin = "ADMIN";
    public const string User = "USER";
    public const string ReadOnlyUser = "READONLYUSER";

    public static async Task SeedUsers(WebApplication app) {
      using (var scope = app.Services.CreateScope()) {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<BlazorApp1Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BlazorApp1User>>();

        string[] roles = { SuperAdmin, Admin, User, ReadOnlyUser };
        foreach (string role in roles) {
          if (!(await roleManager.RoleExistsAsync(role))) {
            await roleManager.CreateAsync(new BlazorApp1Role() { Name = role});
          }
        }
        // --- define super admin!
        await SeedUser("info@eswys.ch", "1234", new string[] { SuperAdmin, Admin, User, ReadOnlyUser }, userManager);
        await SeedUser("user@eswys.ch", "1234", new string[] { User, ReadOnlyUser }, userManager);
        // --- test: does the user exist after it was created?
        await SeedUser("info@eswys.ch", "1234", new string[] { SuperAdmin, Admin, User, ReadOnlyUser }, userManager);
      }
    }

    static async Task SeedUser(string email, string password, string[] roles, UserManager<BlazorApp1User> userManager) {
      // Create the SuperAdmin user if it doesn't exist
      BlazorApp1User user = await userManager.FindByEmailAsync(email);
      if (user == null) {
        user = new BlazorApp1User {
          UserName = email,
          NormalizedUserName = email.ToUpperInvariant().Trim(),
          Email = email,
          NormalizedEmail = email.ToUpperInvariant().Trim(),
          EmailConfirmed = true
        };
        try {
          var result = await userManager.CreateAsync(user, password); // Set a default password
          if (!result.Succeeded) {
            Console.WriteLine("Something went wrong: " + result.ToString());
          }
        }
        catch (Exception ex) {
          Console.WriteLine(ex.ToString());
        }
      }
      else {
        Console.WriteLine("Found user...");
      }
      /*
      foreach (string role in roles) {
        if (!(await userManager.IsInRoleAsync(user, role))) {
          // User is in the specified role
          await userManager.AddToRoleAsync(user, role); // Assign the Readonly role
        }
      }*/
    }
  }
}
