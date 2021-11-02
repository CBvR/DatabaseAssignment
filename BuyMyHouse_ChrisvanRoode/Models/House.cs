using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Models
{
	[OpenApiExample(typeof(DummyHouseExample))]
	public class House
	{
		[OpenApiProperty(Description = "Gets or sets the house ID.")]
		[JsonRequired]
		public int houseId { get; set; }

		[OpenApiProperty(Description = "Gets or sets the house prive.")]
		[JsonRequired]
		public int price { get; set; }

		[OpenApiProperty(Description = "Gets or sets the house adres.")]
		[JsonRequired]
		public string adres { get; set; }

		[OpenApiProperty(Description = "Gets or sets the house picture which is saved in the blob.")]
		public string photoUrl { get; set; }
	}

	public class DummyHouseExample : OpenApiExample<House>
	{
		public override IOpenApiExample<House> Build(NamingStrategy NamingStrategy = null)
		{
			Examples.Add(OpenApiExampleResolver.Resolve("Heerenweg 8", new House() { houseId = 1, price = 349000, adres = "Heerenweg 8" }, NamingStrategy));
			Examples.Add(OpenApiExampleResolver.Resolve("Stationsweg 25", new House() { houseId = 2, price = 290000, adres = "Stationsweg 25"}, NamingStrategy));

			return this;
		}
	}

	public class DummyHouseExamples : OpenApiExample<List<House>>
	{
		public override IOpenApiExample<List<House>> Build(NamingStrategy NamingStrategy = null)
		{
			Examples.Add(OpenApiExampleResolver.Resolve("Houses", new List<House> {
				new House() { houseId = 1, price = 349000, adres = "Heerenweg 8"  },
				new House() { houseId = 2, price = 290000, adres = "Stationsweg 25" },
			}));

			return this;
		}
	}
}
