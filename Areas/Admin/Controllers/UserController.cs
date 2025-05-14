using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleRestaurant.Models;

namespace SimpleRestaurant.Areas.Admin.Controllers
{
	[Authorize(Roles = "Admin, Developer")]
	[Area("Admin")]
	public class UserController(UserManager<ApplicationUser> user, RoleManager<IdentityRole> role) : Controller
	{
		private UserManager<ApplicationUser> userManager = user;
		private RoleManager<IdentityRole> roleManager = role;

		public async Task<IActionResult> Index()
		{
			List<ApplicationUser> users = new List<ApplicationUser>();

			foreach (ApplicationUser user in userManager.Users)
			{
				user.RoleNames = await userManager.GetRolesAsync(user);
				users.Add(user);
			}

			UserViewModel model = new()
			{
				Users = users,
				Roles = roleManager.Roles
			};

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(string id)
		{
			ApplicationUser? user = await userManager.FindByIdAsync(id);
			if (user != null)
			{
				IdentityResult result = await userManager.DeleteAsync(user);
				if (!result.Succeeded)
				{
					string errorMessage = result.Errors.Aggregate("", (current, error) => current + (error.Description + " | "));
					//foreach (IdentityError error in result.Errors)
					//{
					//	errorMessage += error.Description + " | ";
					//} same as above
					TempData["message"] = errorMessage;
				}
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> AddToRole(string userId, string roleName)
		{
			IdentityRole? role = await roleManager.FindByNameAsync(roleName);
			if (role != null)
			{
				ApplicationUser? user = await userManager.FindByIdAsync(userId);
				if (user != null)
					await userManager.AddToRoleAsync(user, roleName);
			}
			else
				TempData["message"] = $"Role {roleName} doesn't exist!";

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> RemoveFromRole(string userId, string roleName)
		{
			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user != null)
				await userManager.RemoveFromRoleAsync(user, roleName);
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> CreateRole(string roleName)
		{
			if (!string.IsNullOrEmpty(roleName))
			{
				IdentityRole role = new()
				{
					Name = roleName
				};
				IdentityResult result = await roleManager.CreateAsync(role);
				if (result.Succeeded)
					TempData["message"] = $"Role {roleName} created!";
				else
					TempData["message"] = "Error creating role!";
			}
			else
				TempData["message"] = "Role name cannot be empty!";
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> CreateAdminRole()
		{
			await roleManager.CreateAsync(new IdentityRole("Admin"));
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> DeleteRole(string id)
		{
			IdentityRole? role = await roleManager.FindByIdAsync(id);
			if (role != null)
			{
				IdentityResult result = await roleManager.DeleteAsync(role);
				if (result.Succeeded)
					TempData["message"] = $"Role {role.Name} deleted!";
				else
					TempData["message"] = "Error deleting role!";
			}
			else
				TempData["message"] = "Role not found!";
			return RedirectToAction("Index");
		}
	}
}
