using BlazorApp1.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using BlazorApp1.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Protocol.Plugins;

//ActiveDirectory
using System.DirectoryServices.AccountManagement;

//Single sign-on (sso)
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using BlazorApp1.Support;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
//using Microsoft.AspNetCore.Http;

//SendGrid
using WebPWrecover.Services;

namespace BlazorApp1 {
  public class Program {
    public static async Task Main(string[] args) {
      var builder = WebApplication.CreateBuilder(args);
      var connectionString = builder.Configuration.GetConnectionString("BlazorApp1ContextConnection") ?? throw new InvalidOperationException("Connection string 'BlazorApp1ContextConnection' not found.");
      var configuration = builder.Configuration;
      builder.Services.AddDbContext<BlazorApp1Context>(options => options.UseSqlite(connectionString));

     // ActiveDirectory 
     builder.Services.AddSingleton(provider => new PrincipalContext(ContextType.Domain, "eswys.local"));

     //Single sign-on (sso)
     builder.Services.ConfigureSameSiteNoneCookies();

     builder.Services.AddAuth0WebAppAuthentication(options =>
     {
         options.Domain = builder.Configuration["Auth0:Domain"];
         options.ClientId = builder.Configuration["Auth0:ClientId"];
     });
     builder.Services.AddControllersWithViews();

     // SendGrid
     builder.Services.AddTransient<IEmailSender, EmailSender>();
     builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

         
            builder.Services.AddIdentity<BlazorApp1User, BlazorApp1Role>(options => {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 4;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        // --- NEG_TODO: add email confirmation!!!
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
        // many different options to setup here!!!
      }).AddEntityFrameworkStores<BlazorApp1Context>();

      // Add services to the container.
      builder.Services.AddRazorPages();
      builder.Services.AddServerSideBlazor();
      builder.Services.AddSingleton<WeatherForecastService>();

          
      var app = builder.Build();

      // --- Seed data
      await UserSeed.SeedUsers(app);


      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment()) {
        app.UseMigrationsEndPoint();
      }
      else {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();

      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
      app.MapRazorPages();
      // --- was...
      app.MapBlazorHub();
      app.MapFallbackToPage("/_Host");

      app.Run();
    }
  }
}
