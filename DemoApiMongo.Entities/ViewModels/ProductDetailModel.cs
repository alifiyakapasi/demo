namespace DemoApiMongo.Entities.ViewModels
{
    public class ProductDetailModel
    {
        public string? Id { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public int ProductPrice { get; set; }
        public int ProductQuantity { get; set; }
        public string? CategoryId { get; set; }
        public string? ProductStatus { get; set; }
        public List<string>? SelectedCategory { get; set; }
        public string? FileUpload { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Time { get; set; }
    }
}
