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
using Task = System.Threading.Tasks.Task;
using MongoDB.Driver;
using Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using DAL;

namespace Services
{
    public interface IHousesService
    {
        Task<House> CreateHouse(House house);
        Task<House> GetHouse(int houseId);
        Task<House> UpdateHouse(House house);
        Task<House> AddImage(int houseId, string photoUrl);
        Task<IEnumerable<House>> GetAllHouses();
        Task<IEnumerable<House>> GetHousesWithBudget(int from, int to);
        Task DeleteHouse(int houseId);
    }

    public class HouseService : IHousesService
    {
        private readonly IHouseRepository _houses;

        public HouseService(ILogger<HouseService> Logger, IHouseRepository houseRepository)
        {
            this._houses = houseRepository;
        }

        public Task<House> CreateHouse(House house)
        {
            if (_houses.GetHouse(house.houseId) == null)
            {
                _houses.CreateHouse(house);
                return Task.FromResult(house);
            }
            return Task.FromResult<House>(null);
        }

        public Task<House> GetHouse(int houseId)
        {
            BsonDocument retrievedHouse = _houses.GetHouse(houseId);
            if (retrievedHouse == null) return Task.FromResult<House>(null);
            retrievedHouse.Remove("_id");
            return Task.FromResult(BsonSerializer.Deserialize<House>(retrievedHouse));
        }

        public Task<House> UpdateHouse(House house)
        {
            _houses.UpdateHouse(BsonDocument.Parse(house.ToJson()), house.houseId);
            BsonDocument newHouse = _houses.GetHouse(house.houseId);
            newHouse.Remove("_id");
            return Task.FromResult(BsonSerializer.Deserialize<House>(newHouse));
        }

        public Task<House> AddImage(int houseId, string photoUrl)
        {
            BsonDocument retrievedHouse = _houses.GetHouse(houseId);
            retrievedHouse.Remove("_id");
            House house = BsonSerializer.Deserialize<House>(retrievedHouse);
            if (house == null) return Task.FromResult<House>(null);
            else
            {
                house.photoUrl = photoUrl;
                _houses.UpdateHouse(BsonDocument.Parse(house.ToJson()), houseId);
                BsonDocument newHouse = _houses.GetHouse(houseId);
                newHouse.Remove("_id");
                return Task.FromResult(BsonSerializer.Deserialize<House>(newHouse));
            }
        }

        public Task DeleteHouse(int houseId)
        {
            if (_houses.GetHouse(houseId) == null) return Task.FromResult(false);
            return Task.FromResult(_houses.DeleteHouse(houseId));
        }

        public Task<IEnumerable<House>> GetAllHouses()
        {
            List<House> houses = new List<House>();
            IEnumerable<BsonDocument> bsonHouses = _houses.GetAllHouses();
            foreach (BsonDocument bson in bsonHouses)
            {
                bson.Remove("_id");
                House newHouse = BsonSerializer.Deserialize<House>(bson);
                houses.Add(newHouse);
            };

            IEnumerable<House> newList = houses;
            return Task.FromResult(newList);
        }

        public Task<IEnumerable<House>> GetHousesWithBudget(int from, int to)
        {
            List<House> houses = new List<House>();
            IEnumerable<BsonDocument> bsonHouses = _houses.GetHousesWithBudget(from, to);
            foreach (BsonDocument bson in bsonHouses)
            {
                bson.Remove("_id");
                House newHouse = BsonSerializer.Deserialize<House>(bson);
                houses.Add(newHouse);
            };

            IEnumerable<House> newList = houses;
            return Task.FromResult(newList);
        }
    }
}