using System.Threading.Tasks;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;

namespace DAL
{
    public interface IHouseRepository
    {
        House CreateHouse(House house);
        BsonDocument GetHouse(int houseId);
        void UpdateHouse(BsonDocument HouseToUpdate, int houseId);
        IEnumerable<BsonDocument> GetAllHouses();
        IEnumerable<BsonDocument> GetHousesWithBudget(int from, int to);
        bool DeleteHouse(int houseId);
    }

    public class HouseRepository : IHouseRepository
    {
        public House CreateHouse(House house)
        {
            IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("houses");
            collection.InsertOne(BsonDocument.Parse(house.ToJson()));
            return house;
        }

        public BsonDocument GetHouse(int houseId)
        {
            IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("houses");
            var filter = Builders<BsonDocument>.Filter.Eq("houseId", houseId);
            var house = collection.Find(filter).FirstOrDefault();
            return house;
        }

        public void UpdateHouse(BsonDocument houseToUpdate, int houseId)
        {
            IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("houses");
            var filter = Builders<BsonDocument>.Filter.Eq("houseId", houseId);
            collection.ReplaceOne(filter, houseToUpdate);
        }

        public IEnumerable<BsonDocument> GetAllHouses()
        {
            IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("houses");
            IEnumerable<BsonDocument> documents = collection.Find(_ => true).ToList();
            return documents;
        }

        public IEnumerable<BsonDocument> GetHousesWithBudget(int from, int to)
        {
            IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("houses");
            var filter = Builders<BsonDocument>.Filter.Lt("price", to) & Builders<BsonDocument>.Filter.Gt("price", from);
            IEnumerable<BsonDocument> documents = collection.Find(filter).ToList();
            return documents;
        }

        public bool DeleteHouse(int houseId)
        {
            try
            {
                IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("houses");
                var filter = Builders<BsonDocument>.Filter.Eq("houseId", houseId);
                var house = collection.DeleteOne(filter);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}