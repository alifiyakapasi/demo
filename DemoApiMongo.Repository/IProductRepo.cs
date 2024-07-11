using DemoApiMongo.Entities.DataModels;
using DemoApiMongo.Entities.ViewModels;
using Microsoft.AspNetCore.JsonPatch;

namespace DemoApiMongo.Repository
{
    public interface IProductRepo
    {
        public Task<List<ProductDetails>> ProductListAsync();

        public Task<ProductDetails> GetProductDetailByIdAsync(string productId);

        public Task<List<ProductDetails>> GetProductDetailsByNameAsync(string name);

        public Task AddProductAsync(ProductDetailModel productDetails);

        public Task UpdateProductAsync(string productId, ProductDetails productDetails);

        public Task<ProductDetails> UpdatePartialProductAsync(string productId, JsonPatchDocument<ProductDetails> patchDocument);
        public Task DeleteProductAsync(String productId);

        public Task InsertProductCategoriesAsync(List<ProductCategories> list);

        public Task AddCategoryAsync(ProductCategoryModel model);

        public Task<List<CategoryList>> CategoryListAsync();
    }
}
