﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleRestaurant.Models
{
	public class Product
	{
		public Product()
		{
			ProductIngredients = new List<ProductIngredient>();
		}

		public int ProductId { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public decimal Price { get; set; }
		public int Stock { get; set; }
		public int CategoryId { get; set; }

		[NotMapped]
		public IFormFile? ImageFile { get; set; }
		public string ImageUrl { get; set; } = "https://via.placeholder.com/150";

		[ValidateNever]
		public Category? Category { get; set; } // A product belongs to a category

		[ValidateNever]
		public ICollection<OrderItem>? OrderItems { get; set; } // A Product can be in many order Items

		[ValidateNever]
		public ICollection<ProductIngredient>? ProductIngredients { get; set; } // A product can have many Ingredient
	}
}