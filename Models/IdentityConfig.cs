using Microsoft.AspNetCore.Identity;

namespace SimpleRestaurant.Models
{
	public class IdentityConfig
	{
		public static async Task CreateAdminUserAsync(IServiceProvider provider)
		{
			var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
			var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

			string username = "admin@admin.com";
			string password = "Admin@123";
			string roleName = "Admin";

			//If role doesn't exist create it
			if (await roleManager.FindByNameAsync(roleName) == null)
			{
				await roleManager.CreateAsync(new IdentityRole(roleName));
			}

			// If user doesn't exist create it and add to role
			if (await userManager.FindByNameAsync(username) == null)
			{
				var user = new ApplicationUser
				{
					UserName = username,
					Email = username,
					EmailConfirmed = true,
					PhoneNumberConfirmed = true
				};
				var result = await userManager.CreateAsync(user, password);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(user, roleName);
				}
			}
		}
	}
}
