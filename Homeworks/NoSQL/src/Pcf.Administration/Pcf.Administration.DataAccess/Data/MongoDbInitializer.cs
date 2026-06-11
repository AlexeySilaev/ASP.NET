using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Pcf.Administration.Core.Domain.Administration;
using System;

namespace Pcf.Administration.DataAccess.Data
{
    public class MongoDbInitializer : IDbInitializer
    {
        private readonly IMongoCollection<Employee> _employeeCollection;
        private readonly IMongoCollection<Role> _roleCollection;

        public MongoDbInitializer(MongoUrl mongoUrl)
        {
            var dbClient = new MongoClient(mongoUrl);
            var db = dbClient.GetDatabase(mongoUrl.DatabaseName);
            _employeeCollection = db.GetCollection<Employee>($"{typeof(Employee).Name.ToLower()}s");
            _roleCollection = db.GetCollection<Role>($"{typeof(Role).Name.ToLower()}s");
        }

        public void InitializeDb()
        {
            if (_employeeCollection.CountDocuments(_ => true) == 0)
            {
                //   следующий метод отказывается работать и вызывает исключение, мол, сериалайзер уже зарегистрирован
                //   BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
                _employeeCollection.InsertMany(FakeDataFactory.Employees);
                _roleCollection.InsertMany(FakeDataFactory.Roles);
            }
        }
    }
}
