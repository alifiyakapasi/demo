using MongoDB.Bson.Serialization.Attributes;

namespace DemoApiMongo.Entities.DataModels
{
    public class ProductCategories
    {
        public string ProductId { get; set; }
        [BsonElement("ProductName")]
        public string ProductName { get; set; }
        public List<string> ProductType { get; set; }
        public string TotalItems { get; set; }
    }
}
