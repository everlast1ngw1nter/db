using System;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            return userCollection
                .Find(x => x.Id == id)
                .FirstOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            var user = FindByLogin(login);
            if (user != null)
            {
                return user;
            }
            user = new UserEntity();
            user.Login = login;
            return Insert(user);
        }

        private UserEntity FindByLogin(string login)
        {
            return userCollection
                .Find(x => x.Login == login)
                .FirstOrDefault();
        }

        public void Update(UserEntity user)
        {
            var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, user.Id);
            userCollection.ReplaceOne(filter, user);
        }

        public void Delete(Guid id)
        {
            var filter = Builders<UserEntity>.Filter.Eq(u => u.Id, id);
            userCollection.DeleteOne(filter);
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var users = userCollection
                .Find(_ => true)
                .SortBy(u => u.Login)
                .Skip((pageNumber - 1) * pageSize) 
                .Limit(pageSize) 
                .ToList();

            var totalCount = userCollection.CountDocuments(_ => true);
            return new PageList<UserEntity>(users, totalCount, pageNumber, pageSize);
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}