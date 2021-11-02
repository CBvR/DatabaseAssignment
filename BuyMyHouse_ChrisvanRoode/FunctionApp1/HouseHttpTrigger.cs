using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Models;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.Functions.Worker; 
using Microsoft.Extensions.Logging;
using System.IO;
using Newtonsoft.Json;
using Services;
using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Resolvers;
using Newtonsoft.Json.Serialization;
using System.Collections;

namespace BuyMyHouse {
    public class HouseHttpTrigger {
		ILogger Logger { get; }
		private IHousesService _houses { get; }
		private IBlobService _blobs { get; }

		public HouseHttpTrigger(IHousesService HousesService, IBlobService BlobsService, ILogger<HouseHttpTrigger> Logger) {
			this.Logger = Logger;
			this._houses = HousesService;
			this._blobs = BlobsService;
		}

		[Function(nameof(HouseHttpTrigger.AddHouse))]
		[OpenApiOperation(operationId: "addHouse", tags: new[] { "house" }, Summary = "Add a new house to the database", Description = "This add a new house to the database.", Visibility = OpenApiVisibilityType.Important)]
		//[OpenApiSecurity("housedatabase_auth", SecuritySchemeType.Http, In = OpenApiSecurityLocationType.Header, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(House), Example = typeof(DummyHouseExample), Required = true, Description = "House object that needs to be added to the database")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(House), Summary = "New house details added", Description = "New house details added", Example = typeof(DummyHouseExample))]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Conflict, Summary = "House id already exists", Description = "There is already a house with this userid")]
		public async Task<HttpResponseData> AddHouse([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "house")] HttpRequestData req, FunctionContext executionContext) {
			// Parse input
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			House house = JsonConvert.DeserializeObject<House>(requestBody);
			house = await _houses.CreateHouse(house);

			HttpResponseData response;
			// Generate output
			if (house != null) response = req.CreateResponse(HttpStatusCode.OK);
			else response = req.CreateResponse(HttpStatusCode.Conflict);

			await response.WriteAsJsonAsync(house);

			return response;
		}

		[Function(nameof(HouseHttpTrigger.AddHouseImage))]
		[OpenApiOperation(operationId: "addHouseImage", tags: new[] { "house" }, Summary = "Adds an houseImage to the database", Description = "Adds an houseImage to the database.", Visibility = OpenApiVisibilityType.Important)]
		//[OpenApiSecurity("housedatabase_auth", SecuritySchemeType.Http, In = OpenApiSecurityLocationType.Header, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		[OpenApiParameter(name: "houseId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Summary = "From price of desired house", Description = "From price of desired house, can be empty to start from zero (0)", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiParameter(name: "photoUrl", In = ParameterLocation.Path, Required = true, Type = typeof(string), Summary = "local url for Photo from input", Description = "Photo from input", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(House), Summary = "New house details added", Description = "New house details added", Example = typeof(DummyHouseExample))]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Conflict, Summary = "House id already exists", Description = "There is already a house with this userid")]
		public async Task<HttpResponseData> AddHouseImage([HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "house/addImage/{houseId}/{photoUrl}")] HttpRequestData req, int houseId, string photoUrl, FunctionContext executionContext)
		{
			// Parse input
			
			HttpResponseData response;

			// Generate output
			if (_houses.GetHouse(houseId) != null)
			{
				string photoWithoutExt = _blobs.AddFile(photoUrl, "images");
				await _houses.AddImage(houseId, photoWithoutExt);
				response = req.CreateResponse(HttpStatusCode.OK);
			}
			else response = req.CreateResponse(HttpStatusCode.Conflict);

			return response;
		}

		[Function(nameof(HouseHttpTrigger.UpdateHouse))]
		[OpenApiOperation(operationId: "updateHouse", tags: new[] { "house" }, Summary = "Update an existing house", Description = "This updates an existing house.", Visibility = OpenApiVisibilityType.Important)]
		//[OpenApiSecurity("housedatabase_auth", SecuritySchemeType.Http, In = OpenApiSecurityLocationType.Header, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		[OpenApiRequestBody(contentType: "application/json", bodyType: typeof(House), Required = true, Description = "House object that needs to be updated to the database")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(House), Summary = "House details updated", Description = "House details updated", Example = typeof(DummyHouseExample))]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Invalid ID supplied", Description = "Invalid ID supplied")]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "House not found", Description = "House not found")]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.MethodNotAllowed, Summary = "Validation exception", Description = "Validation exception")]
		public async Task<HttpResponseData> UpdateHouse([HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "house")] HttpRequestData req, FunctionContext executionContext)
		{
			// Parse input
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			House house = JsonConvert.DeserializeObject<House>(requestBody);

			house = await _houses.UpdateHouse(house);

			// Generate output
			HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

			await response.WriteAsJsonAsync(house);

			return response;
		}

		[Function(nameof(HouseHttpTrigger.GetHouseById))]
		[OpenApiOperation(operationId: "getHouseById", tags: new[] { "house" }, Summary = "Find house by ID", Description = "Returns a single house.", Visibility = OpenApiVisibilityType.Important)]
		//[OpenApiSecurity("housedatabase_auth", SecuritySchemeType.Http, In = OpenApiSecurityLocationType.Header, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		[OpenApiParameter(name: "houseId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Summary = "ID of house to return", Description = "ID of house to return", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(House), Summary = "successful operation", Description = "successful operation", Example = typeof(DummyHouseExamples))]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Invalid ID supplied", Description = "Invalid ID supplied")]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "House not found", Description = "House not found")]
		public async Task<HttpResponseData> GetHouseById([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "house/{houseId}")] HttpRequestData req, int houseId, FunctionContext executionContext)
		{
			// Generate output
			HttpResponseData response;

			House house = await _houses.GetHouse(houseId);

			if (house == null)
			{
				response = req.CreateResponse(HttpStatusCode.BadRequest);


			}
			else
			{
				response = req.CreateResponse(HttpStatusCode.OK);
				await response.WriteAsJsonAsync(house);
			}

			return response;
		}

		[Function(nameof(HouseHttpTrigger.GetHouseWithBudget))]
		[OpenApiOperation(operationId: "getHouseWithBudget", tags: new[] { "house" }, Summary = "Returns houses within budget limits", Description = "Returns houses within budget limits.", Visibility = OpenApiVisibilityType.Important)]
		//[OpenApiSecurity("housedatabase_auth", SecuritySchemeType.Http, In = OpenApiSecurityLocationType.Header, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		[OpenApiParameter(name: "from", In = ParameterLocation.Path, Required = false, Type = typeof(int), Summary = "From price of desired house", Description = "From price of desired house, can be empty to start from zero (0)", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiParameter(name: "to", In = ParameterLocation.Path, Required = true, Type = typeof(int), Summary = "To price of desired house", Description = "To price of desired house", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(House), Summary = "successful operation", Description = "successful operation", Example = typeof(DummyHouseExamples))]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Invalid ID supplied", Description = "Invalid ID supplied")]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "House not found", Description = "House not found")]
		public async Task<HttpResponseData> GetHouseWithBudget([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "house/{from}/{to}")] HttpRequestData req, int to, FunctionContext executionContext, int from = 0)
		{
			// Generate output
			HttpResponseData response;

			IEnumerable<House> houses = await _houses.GetHousesWithBudget(from, to);

			if (houses == null)
			{
				response = req.CreateResponse(HttpStatusCode.BadRequest);


			}
			else
			{
				response = req.CreateResponse(HttpStatusCode.OK);
				await response.WriteAsJsonAsync(houses);
			}

			return response;
		}

		[Function(nameof(HouseHttpTrigger.GetHouses))]
		[OpenApiOperation(operationId: "getHouseById", tags: new[] { "house" }, Summary = "Get all houses from the system", Description = "Recovers all the houses from the system <br />This should only be available to the direction", Visibility = OpenApiVisibilityType.Important)]
		//[OpenApiSecurity("housedatabase_auth", SecuritySchemeType.Http, In = OpenApiSecurityLocationType.Header, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(House), Summary = "successful operation", Description = "successful operation", Example = typeof(DummyHouseExamples))]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Invalid ID supplied", Description = "Invalid ID supplied")]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "House not found", Description = "House not found")]
		public async Task<HttpResponseData> GetHouses([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "house")] HttpRequestData req, FunctionContext executionContext)
		{
			// Generate output
			HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

			IEnumerable<House> houses = await _houses.GetAllHouses();

			await response.WriteAsJsonAsync(houses);

			return response;
		}

		[Function(nameof(HouseHttpTrigger.DeleteHouse))]
		[OpenApiOperation(operationId: "deleteHouse", tags: new[] { "house" }, Summary = "Find house by ID", Description = "Returns a single house.", Visibility = OpenApiVisibilityType.Important)]
		//[OpenApiSecurity("housedatabase_auth", SecuritySchemeType.Http, In = OpenApiSecurityLocationType.Header, Scheme = OpenApiSecuritySchemeType.Bearer, BearerFormat = "JWT")]
		[OpenApiParameter(name: "houseId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Summary = "ID of house to return", Description = "ID of house to return", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(House), Summary = "successful operation", Description = "successful operation", Example = typeof(DummyHouseExamples))]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Invalid ID supplied", Description = "Invalid ID supplied")]
		[OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Summary = "House not found", Description = "House not found")]
		public async Task<HttpResponseData> DeleteHouse([HttpTrigger(AuthorizationLevel.Anonymous, "DELETE", Route = "house/{houseId}")] HttpRequestData req, int houseId, FunctionContext executionContext)
		{
			// Generate output
			HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

			await _houses.DeleteHouse(houseId);

			return response;
		}
	}
}

