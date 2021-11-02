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
    public interface IUserRepository
    {
        User CreateUser(User user);
        BsonDocument GetUser(int userId);
        IEnumerable<BsonDocument> GetAllUsers();
        void UpdateUser(User user);
        bool DeleteUser(int userId); 
    }

    public class UserRepository : IUserRepository
    {
        public User CreateUser(User user)
        {
            IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("users");
            collection.InsertOne(BsonDocument.Parse(user.ToJson()));
            return user;
        }

        public BsonDocument GetUser(int userId)
        {
            IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("users");
            var filter = Builders<BsonDocument>.Filter.Eq("UserId", userId);
            var user = collection.Find(filter).FirstOrDefault();
            return user;
        }

        public IEnumerable<BsonDocument> GetAllUsers()
        {
            IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("users");
            IEnumerable<BsonDocument> documents = collection.Find(_ => true).ToList();
            return documents;
        }

        public void UpdateUser(User user)
        {
            IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("users");
            var filter = Builders<BsonDocument>.Filter.Eq("UserId", user.userId);
            collection.ReplaceOne(filter, BsonDocument.Parse(user.ToJson()));
        }

        public bool DeleteUser(int userId)
        {
            try
            {
                IMongoCollection<BsonDocument> collection = MongoSingleton.getMongoCollection("users");
                var filter = Builders<BsonDocument>.Filter.Eq("UserId", userId);
                var user = collection.DeleteOne(filter);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}