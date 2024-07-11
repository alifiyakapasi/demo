using Boxed.Mapping;
using DemoApiMongo.Entities.DataModels;
using DemoApiMongo.Entities.ViewModels;

namespace DemoApiMongo.Entities.Mappers
{
    public class BoxedMapping : IMapper<ProductDetailModel, ProductDetails>
    {
        public void Map(ProductDetailModel source, ProductDetails destination)
        {
            destination.Id = source.Id;
            destination.ProductName = source.ProductName;
            destination.ProductDescription = source.ProductDescription;
            destination.ProductPrice = source.ProductPrice;
            destination.ProductQuantity = source.ProductQuantity;
            destination.CategoryId = source.CategoryId;
            destination.ProductStatus = source.ProductStatus;
            destination.SelectedCategory = source.SelectedCategory;
            destination.FileUpload = source.FileUpload;
            destination.FromDate = source.FromDate;
            destination.ToDate = source.ToDate;
            destination.Time = source.Time;
        }
    }
}
