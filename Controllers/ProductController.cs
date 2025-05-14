using Microsoft.AspNetCore.Mvc;
using SimpleRestaurant.Data;
using SimpleRestaurant.Models;

namespace SimpleRestaurant.Controllers
{
	public class ProductController : Controller
	{
		private readonly Repository<Product> products;
		private readonly Repository<Ingredient> ingredients;
		private readonly Repository<Category> categories;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
		{
			products = new Repository<Product>(context);
			ingredients = new Repository<Ingredient>(context);
			categories = new Repository<Category>(context);
			_webHostEnvironment = webHostEnvironment;
		}

		public async Task<IActionResult> Index()
		{
			return View(await products.GetAllAsync());
		}

		[HttpGet]
		public async Task<IActionResult> AddEdit(int id)
		{
			ViewBag.Ingredients = await ingredients.GetAllAsync();
			ViewBag.Categories = await categories.GetAllAsync();
			if (id == 0)
			{
				ViewBag.Operation = "Add";
				return View(new Product());
			}
			ViewBag.Operation = "Edit";
			Product product = await products.GetByIdAsync(id, new QueryOptions<Product>
			{
				Includes = "ProductIngredients.Ingredient, Category"
			});
			return View(product);
		}

		[HttpPost]
		public async Task<IActionResult> AddEdit(Product product, int[] ingredientIds, int catId)
		{
			ViewBag.Ingredients = await ingredients.GetAllAsync();
			ViewBag.Categories = await categories.GetAllAsync();

			if (!ModelState.IsValid)
				return RedirectToAction("Index", "Product");

			if (product.ImageFile != null)
			{
				string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
				string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;
				string existingFilePath = Path.Combine(uploadsFolder, product.ImageUrl);
				if (System.IO.File.Exists(existingFilePath))
				{
					System.IO.File.Delete(existingFilePath);
				}
				string filePath = Path.Combine(uploadsFolder, uniqueFileName);
				await using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					await product.ImageFile.CopyToAsync(fileStream);
				}
				product.ImageUrl = uniqueFileName;
			}

			if (product.ProductId == 0) // Added New Product
			{

				product.CategoryId = catId;

				//add ingredients
				foreach (int id in ingredientIds)
				{
					product.ProductIngredients?.Add(new ProductIngredient { IngredientId = id, ProductId = product.ProductId });
				}

				await products.AddAsync(product);
				return RedirectToAction("Index", "Product");
			}
			// Edit Existing Product

			var existingProduct = await products.GetByIdAsync(product.ProductId, new QueryOptions<Product> { Includes = "ProductIngredients" });

			if (existingProduct == null)
			{
				ModelState.AddModelError("", "Product not found.");
				ViewBag.Ingredients = await ingredients.GetAllAsync();
				ViewBag.Categories = await categories.GetAllAsync();
				return View(product);
			}

			existingProduct.Name = product.Name;
			existingProduct.Description = product.Description;
			existingProduct.Price = product.Price;
			existingProduct.Stock = product.Stock;
			existingProduct.CategoryId = catId;
			existingProduct.ImageUrl = product.ImageUrl;

			// Update product ingredients
			existingProduct.ProductIngredients?.Clear();
			foreach (int id in ingredientIds)
			{
				existingProduct.ProductIngredients?.Add(new ProductIngredient { IngredientId = id, ProductId = product.ProductId });
			}

			try
			{
				await products.UpdateAsync(existingProduct);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
				ViewBag.Ingredients = await ingredients.GetAllAsync();
				ViewBag.Categories = await categories.GetAllAsync();
				return View(product);
			}
			return RedirectToAction("Index", "Product");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await products.DeleteAsync(id);
				return RedirectToAction("Index", "Product");
			}
			catch
			{
				ModelState.AddModelError("", "Product Not Found!");
				return RedirectToAction("Index", "Product");
			}
		}
	}
}
