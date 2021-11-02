using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Resolvers;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Models
{
	[OpenApiExample(typeof(DummyUserExample))]
	public class User
	{
		[OpenApiProperty(Description = "Gets or sets the user ID.")]
		[JsonRequired]
		public int userId { get; set; }

		[OpenApiProperty(Description = "Gets or sets the name.")]
		[JsonRequired]
		public string name { get; set; }

		[OpenApiProperty(Description = "Gets or sets the user mail.")]
		[JsonRequired]
		public string email { get; set; }

		[OpenApiProperty(Description = "Gets or sets the name.")]
		[JsonRequired]
		public int userIncome { get; set; }

		public int mortgage { get; set; }
	}

	public class DummyUserExample : OpenApiExample<User>
	{
		public override IOpenApiExample<User> Build(NamingStrategy NamingStrategy = null)
		{
			Examples.Add(OpenApiExampleResolver.Resolve("Chris van Roode", new User() { userId = 101, name = "Chris van Roode", userIncome = 2500, email = "cbvroode@gmail.com" }, NamingStrategy));
			Examples.Add(OpenApiExampleResolver.Resolve("Henk Poort", new User() { userId = 102, name = "Henk Poort", userIncome = 6000, email = "henkpoort@gmail.com" }, NamingStrategy));

			return this;
		}
	}

	public class DummyUserExamples : OpenApiExample<List<User>>
	{
		public override IOpenApiExample<List<User>> Build(NamingStrategy NamingStrategy = null)
		{
			Examples.Add(OpenApiExampleResolver.Resolve("Users", new List<User> {
				new User() { userId = 102, name = "Chris van Roode", userIncome = 2500, email = "cbvroode@gmail.com" },
				new User() { userId = 101, name = "Henk Poort", userIncome = 6000, email = "henkpoort@gmail.com" },
			}));

			return this;
		}
	}
}
