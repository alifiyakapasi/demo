using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DemoApiMongo.Entities.DataModels
{
    public class CategoryList
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string CategoryName { get; set; }
    }
}
