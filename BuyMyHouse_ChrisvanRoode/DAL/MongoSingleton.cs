using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MongoDB.Bson;

namespace DAL
{
    public class MongoSingleton
    {
        static MongoClient client { get; set; }

        public static IMongoCollection<BsonDocument> getMongoCollection(string collectionName)
        {
            if (client == null)
            {
                var settings = MongoClientSettings.FromConnectionString(ConfigurationManager.AppSettings["mongoConnection"]);
                client = new MongoClient(settings);
            }
            IMongoDatabase db = client.GetDatabase("BuyMyHouse");
            return db.GetCollection<BsonDocument>(collectionName);
        }
    }
}