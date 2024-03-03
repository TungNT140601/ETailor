using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.WebAPI
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLoggingMiddleware> _logger;

        public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Capture the request information
            var request = context.Request;
            var requestBodyStream = new MemoryStream();
            await request.Body.CopyToAsync(requestBodyStream);
            requestBodyStream.Seek(0, SeekOrigin.Begin);
            var requestBodyText = await new StreamReader(requestBodyStream).ReadToEndAsync();

            // Log the request information
            _logger.LogInformation($"Request: {request.Method} {request.Path} - Body: {requestBodyText}");

            // Capture the response information
            var originalBodyStream = context.Response.Body;
            var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Call the next middleware in the pipeline
            await _next(context);

            // Log the response information
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            _logger.LogInformation($"Response: {context.Response.StatusCode} - Body: {responseBodyText}");

            // Restore the original response body and flush the log
            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
    }
}
