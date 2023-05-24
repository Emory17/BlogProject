using BlogProject.Data;
using BlogProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BlogProject.Data
{
    public static class DataUtility
    {
        private const string _adminRole = "Admin";
        private const string _moderatorRole = "Moderator";
        public static string GetConnectionString(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            return string.IsNullOrEmpty(databaseUrl) ? connectionString! : BuildConnectionString(databaseUrl);
        }
        private static string BuildConnectionString(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var builder = new NpgsqlConnectionStringBuilder()
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            var userManagerSvc = svcProvider.GetRequiredService<UserManager<BlogUser>>();
            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configurationSvc = svcProvider.GetRequiredService<IConfiguration>();

            //Align the database by checking Migration
            await dbContextSvc.Database.MigrateAsync();

            //Seed Application Roles;
            await SeedRolesAsync(roleManagerSvc);

            //Seed Demo User(s)
            await SeedBlogUsersAsync(userManagerSvc, configurationSvc);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManger)
        {
            if(!await roleManger.RoleExistsAsync(_adminRole))
            {
                await roleManger.CreateAsync(new IdentityRole(_adminRole));
            }

            if (!await roleManger.RoleExistsAsync(_moderatorRole))
            {
                await roleManger.CreateAsync(new IdentityRole(_moderatorRole));
            }
        }

        //Demo Users Seed Method
        private static async Task SeedBlogUsersAsync(UserManager<BlogUser> userManager, IConfiguration configuration)
        {
            string? adminEmail = configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
            string? adminPassword = configuration["AdminLoginPassword"] ?? Environment.GetEnvironmentVariable("AdminLoginPassword");

            string? moderatorEmail = configuration["ModeratorLoginEmail"] ?? Environment.GetEnvironmentVariable("ModeratorLoginEmail");
            string? moderatorPassword = configuration["ModeratorLoginPassword"] ?? Environment.GetEnvironmentVariable("ModeratorLoginPassword");

            BlogUser adminUser = new BlogUser()
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Emory",
                LastName = "Soper",
                EmailConfirmed = true
            };

            BlogUser moderatorUser = new BlogUser()
            {
                UserName = moderatorEmail,
                Email = moderatorEmail,
                FirstName = "E",
                LastName = "Soper",
                EmailConfirmed = true
            };

            try
            {
                BlogUser? user = await userManager.FindByEmailAsync(adminUser.Email!);

                if (user == null)
                {
                    await userManager.CreateAsync(adminUser, adminPassword!);
                    await userManager.AddToRoleAsync(adminUser, _adminRole!);
                }

                BlogUser? moduser = await userManager.FindByEmailAsync(moderatorUser.Email!);

                if (user == null)
                {
                    await userManager.CreateAsync(moderatorUser, moderatorPassword!);
                    await userManager.AddToRoleAsync(moderatorUser, _moderatorRole!);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Login User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }
    }
}
