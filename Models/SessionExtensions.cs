﻿using System.Text.Json;

namespace SimpleRestaurant.Models
{
	public static class SessionExtensions
	{
		public static void Set<T>(this ISession session,string key, T value)
		{
			session.SetString(key, JsonSerializer.Serialize(value));
		}

		public static T Get<T>(this ISession session, string key)
		{
			string value = session.GetString(key);
			if (string.IsNullOrEmpty(value))
			{
				return default(T);
			}
			return JsonSerializer.Deserialize<T>(value);
		}
	}
}
