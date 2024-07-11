using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace DemoApiMongo.Filter
{
    public class ExceptionLoggingFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionLoggingFilter> _logger;

        public ExceptionLoggingFilter(ILogger<ExceptionLoggingFilter> logger)
        {
            _logger = logger;
        }

        //public void OnException(ExceptionContext context)
        //{
        //    _logger.LogError(context.Exception, "An unhandled exception occurred.");
        //    Console.WriteLine("An unhandled exception occurred.");
        //}


        // Customizing Exception
        public void OnException(ExceptionContext context)
        {
            var result = new ObjectResult(new
            {
                context.Exception.Message,
                context.Exception.Source,
                ExceptionType = context.Exception.GetType().FullName,
            })
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            // Log the exception
            _logger.LogError("Unhandled exception occurred while executing request: {ex}", context.Exception);
            context.Result = result;
        }
    }
}
