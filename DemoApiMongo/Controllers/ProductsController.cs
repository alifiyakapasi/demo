using DemoApiMongo.Entities.DataModels;
using DemoApiMongo.Entities.ViewModels;
using DemoApiMongo.Filter;
using DemoApiMongo.Repository;
using ExcelDataReader;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DemoApiMongo.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [LogActionFilter]

    public class ProductsController : ControllerBase
    {

        private readonly IProductRepo productService;
        private readonly ILogger<ProductsController> logger;
        private readonly IMemoryCache _memoryCache;
        public string cacheKey = "product";

        public ProductsController(IProductRepo productRepo, ILogger<ProductsController> logger, IMemoryCache memoryCache)
        {
            this.productService = productRepo;
            this.logger = logger;
            this._memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<List<ProductDetails>> Get()
        {
            /// Below All are Exception Methods

            /// DivideByZeroException: Attempted to divide by zero
            //var x = 1 / Convert.ToInt32("0", CultureInfo.InvariantCulture); 

            /// Throws NullReferenceException
            //string myString = null;
            //string result = myString.ToUpper(); 

            /// ArgumentOutOfRangeException - max length of array is 1, but we try to print index 2 so exception occurs. 
            //ArrayList lis = new ArrayList();
            //lis.Add("A");
            //lis.Add("B");
            //Console.WriteLine(lis[2]);

            /// FormatException is thrown when compiled since we have passed a value other than integer
            //string str = "3.5";
            //int res = int.Parse(str);


            ///OverflowException is thrown since we have passed a value that is out of integer(Int32) range.
            //string str = "757657657657657";
            //int res = int.Parse(str);


            /// InvalidCastException - cast operation was not successful because the data types are incompatible.
            //object obj = new object();
            //int i = (int)obj;


            /// KeyNotFoundException is thrown when a key you are finding is not available in the Dictionary collection.
            //var dict = new Dictionary<string, string>() {
            //{"TV", "Electronics"},
            //{"Laptop", "Computers"},
            //};
            //Console.WriteLine(dict["Pen Drive"]);


            logger.LogInformation("Getting All Data");


            // Cached Memory 
            //List<ProductDetails> list;

            //// Cache Service 
            //if (!_memoryCache.TryGetValue(cacheKey, out list))
            //{
            //    list = await productService.ProductListAsync();

            //    _memoryCache.Set(cacheKey, list,
            //        new MemoryCacheEntryOptions()
            //        .SetAbsoluteExpiration(TimeSpan.FromMinutes(1))); // For 1 Minute data will be stored in cached memory
            //}
            //return list;

            return await productService.ProductListAsync();

        }

        [HttpGet("{productId:length(24)}")]
        public async Task<ActionResult<ProductDetails>> Get(string productId)
        {
            try
            {
                var productDetails = await productService.GetProductDetailByIdAsync(productId);
                logger.LogInformation("Getting Searched Data");
                return productDetails;
            }

            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving product with ID ");
                return NotFound(ex.Message);
            }
        }

        [HttpGet("productName")]
        public async Task<IActionResult> SearchProductsByName([FromQuery(Name = "productName")] string productName)
        {
            if (productName == null)
            {
                var product = await productService.ProductListAsync();
                return Ok(product);
            }
            var products = await productService.GetProductDetailsByNameAsync(productName);
            logger.LogInformation("Getting Searched Data");
            return Ok(products);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Post(ProductDetailModel model)
        {
            await productService.AddProductAsync(model);
            logger.LogInformation("Data Added");
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        [HttpPut("{productId:length(24)}")]
        public async Task<IActionResult> Update(string productId, ProductDetails productDetails)
        {
            var productDetail = await productService.GetProductDetailByIdAsync(productId);

            if (productDetail is null)
            {
                logger.LogError("Data Not Found");
                return NotFound();
            }

            productDetails.Id = productDetail.Id;

            await productService.UpdateProductAsync(productId, productDetails);
            logger.LogInformation("Updated Data");
            return Ok();
        }

        [HttpPatch("{productId}")]
        public async Task<IActionResult> UpdateProductAsync(string productId, [FromBody] JsonPatchDocument<ProductDetails> patchDocument)
        {
            var result = await productService.UpdatePartialProductAsync(productId, patchDocument);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }


        [HttpDelete("{productId:length(24)}")]
        public async Task<IActionResult> Delete(string productId)
        {
            var productDetails = await productService.GetProductDetailByIdAsync(productId);

            if (productDetails is null)
            {
                return NotFound();
            }

            await productService.DeleteProductAsync(productId);
            logger.LogInformation("Data Deleted Successfully");
            return Ok();
        }

        #region Excel & Csv File
        [Route("api/ImportProductCategoryExcelAsync")]
        [HttpPost]
        public async Task<ActionResult> ImportProductCategoryExcelAsync()
        {
            var documentcode = new List<ProductCategories>();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles", "ProductCategory.xlsx");
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read()) //Each row of the file
                    {
                        var strId = (reader.GetValue(0) ?? "").ToString();
                        var strName = (reader.GetValue(1) ?? "").ToString();
                        var strType = (reader.GetValue(2) ?? "").ToString();
                        var strItem = (reader.GetValue(3) ?? "").ToString();


                        var strAdditionalTypes = new List<string>();
                        if (strType != null)
                        {
                            strType = strType.Replace(" ", "");
                            strAdditionalTypes = strType.Split(",").ToList();
                        }

                        var declarationCategoryItem = new ProductCategories()
                        {
                            ProductId = strId,
                            ProductName = strName,
                            ProductType = strAdditionalTypes,
                            TotalItems = strItem,
                        };
                        documentcode.Add(declarationCategoryItem);
                    }

                    if (documentcode != null && documentcode.Count > 1)
                    {
                        documentcode.RemoveAt(0);
                        await productService.InsertProductCategoriesAsync(documentcode);
                    }
                }
            }
            return Ok();
        }


        [Route("api/ImportProductCategoryCsvAsync")]
        [HttpPost]
        public async Task<ActionResult> ImportProductCategoryCsvAsync()
        {
            var documentcode = new List<ProductCategories>();
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles", "ProductCategory1.csv");
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                // here difference occurs in xlsx & csv - CreateCsvReader
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    while (reader.Read()) //Each row of the file
                    {
                        var strId = (reader.GetValue(0) ?? "").ToString();
                        var strName = (reader.GetValue(1) ?? "").ToString();
                        var strType = (reader.GetValue(2) ?? "").ToString();
                        var strItem = (reader.GetValue(3) ?? "").ToString();


                        var strAdditionalTypes = new List<string>();
                        if (strType != null)
                        {
                            strType = strType.Replace(" ", "");
                            strAdditionalTypes = strType.Split(",").ToList();
                        }

                        var declarationCategoryItem = new ProductCategories()
                        {
                            ProductId = strId,
                            ProductName = strName,
                            ProductType = strAdditionalTypes,
                            TotalItems = strItem,
                        };
                        documentcode.Add(declarationCategoryItem);
                    }

                    // here difference occurs in xlsx & csv - Count > 0
                    if (documentcode != null && documentcode.Count > 0)
                    {
                        await productService.InsertProductCategoriesAsync(documentcode);
                    }
                }
            }
            return Ok();
        }

        #endregion


        #region Category
        [HttpPost("addCategory")]
        public async Task<IActionResult> AddCategory(ProductCategoryModel category)
        {
            await productService.AddCategoryAsync(category);
            logger.LogInformation("Data Added");
            return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
        }

        [HttpGet("getcategory")]
        public async Task<List<CategoryList>> GetCategory()
        {
            logger.LogInformation("Getting All Data");
            return await productService.CategoryListAsync();
        }

        #endregion
    }
}
