namespace DemoApiMongo.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class Middleware
    {
        private readonly RequestDelegate _next;

        public Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            Console.WriteLine("Middleware Invoked");
            string operation = "";
            if (httpContext.Request.Method == HttpMethods.Get)
            {
                operation = "Get";
            }
            else if (httpContext.Request.Method == HttpMethods.Post)
            {
                operation = "Create";
            }
            else if (httpContext.Request.Method == HttpMethods.Put)
            {
                operation = "Update";
            }
            else if (httpContext.Request.Method == HttpMethods.Delete)
            {
                operation = "Delete";
            }

            if (!string.IsNullOrEmpty(operation))
            {
                Console.WriteLine($"CRUD Operation: {operation} - {httpContext.Request.Path}");
            }
            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Middleware>();
        }
    }
}
