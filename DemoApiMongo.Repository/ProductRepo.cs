using AutoMapper;
using Boxed.Mapping;
using DemoApiMongo.Configuration;
using DemoApiMongo.Entities.DataModels;
using DemoApiMongo.Entities.ViewModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DemoApiMongo.Repository
{
    public class ProductRepo : IProductRepo
    {
       private readonly IMongoCollection<ProductDetails> _productCollection;
        private readonly IMongoCollection<ProductCategories> _productC;
        private readonly IMongoCollection<CategoryList> _categoryCollection;
        private readonly IMapper _mapper;
        private readonly IMapper<ProductDetailModel, ProductDetails> _BoxedMapper;

        public ProductRepo(
         IOptions<ProductDBSettings> productDatabaseSetting, IMapper mapper, IMapper<ProductDetailModel, ProductDetails> BoxedMapper)
        {
            var mongoClient = new MongoClient(productDatabaseSetting.Value.ConnectionString);
            var database = mongoClient.GetDatabase(productDatabaseSetting.Value.DatabaseName);
           // _productCollection = database.GetCollection<ProductDetails>(productDatabaseSetting.Value.CollectionName);
            _productCollection = database.GetCollection<ProductDetails>("ProductDetails"); 
            _productC = database.GetCollection<ProductCategories>("ProductCategories");
            _categoryCollection = database.GetCollection<CategoryList>("CategoryList");
            _mapper = mapper;
            _BoxedMapper = BoxedMapper;
        }

        public async Task<List<ProductDetails>> ProductListAsync()
        {
            return await _productCollection.Find(_ => true).ToListAsync();
        }

        public async Task<ProductDetails> GetProductDetailByIdAsync(string productId)
        {
            return await _productCollection.Find(x => x.Id == productId).FirstOrDefaultAsync();
        }

        public async Task<List<ProductDetails>> GetProductDetailsByNameAsync(string name)
        {
            var filter = Builders<ProductDetails>.Filter.Regex(p => p.ProductName, name.ToLower());

            var result = await _productCollection.Find(filter).ToListAsync();
            return result;
        }

        public async Task AddProductAsync(ProductDetailModel model)
        {
            //BoxMapping
            ProductDetails productDetails = new ProductDetails();
            _BoxedMapper.Map(model, productDetails);

            //AutoMapping
            //var productDetail = _mapper.Map<ProductDetails>(model);
            await _productCollection.InsertOneAsync(productDetails);
        }

        public async Task UpdateProductAsync(string productId, ProductDetails productDetails)
        {
            await _productCollection.ReplaceOneAsync(x => x.Id == productId, productDetails);
        }

        public async Task<ProductDetails> UpdatePartialProductAsync(string productId, [FromBody] JsonPatchDocument<ProductDetails> patchDocument)
        {
            var filter = Builders<ProductDetails>.Filter.Eq(x => x.Id, productId);
            var update = Builders<ProductDetails>.Update.Combine((IEnumerable<UpdateDefinition<ProductDetails>>)patchDocument);

            var result = await _productCollection.FindOneAndUpdateAsync(filter, update);

            return result;
        }


        public async Task DeleteProductAsync(string productId)
        {
            await _productCollection.DeleteOneAsync(x => x.Id == productId);
        }

        public async Task InsertProductCategoriesAsync(List<ProductCategories> list)
        {
            await _productC.InsertManyAsync(list);
        }


        public async Task AddCategoryAsync(ProductCategoryModel model)
        {
            var category = _mapper.Map<CategoryList>(model);
            await _categoryCollection.InsertOneAsync(category);
        }

        public async Task<List<CategoryList>> CategoryListAsync()
        {
            return await _categoryCollection.Find(_ => true).ToListAsync();
        }
    }
}
