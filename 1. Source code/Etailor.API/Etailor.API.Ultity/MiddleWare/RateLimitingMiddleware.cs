using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etailor.API.Ultity.MiddleWare
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimitOptions _options;
        private readonly IDictionary<string, Queue<DateTime>> _requests = new Dictionary<string, Queue<DateTime>>();

        public RateLimitingMiddleware(RequestDelegate next, RateLimitOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress.ToString();

            if (!_requests.ContainsKey(ipAddress))
            {
                _requests[ipAddress] = new Queue<DateTime>();
            }

            var requestQueue = _requests[ipAddress];
            var now = DateTime.Now;

            // Remove expired requests
            while (requestQueue.Count > 0 && now - requestQueue.Peek() > _options.Interval)
            {
                requestQueue.Dequeue();
            }

            // Check if rate limit exceeded
            if (requestQueue.Count >= _options.Limit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded.");
                return;
            }

            // Add current request to the queue
            requestQueue.Enqueue(now);

            await _next(context);
        }
    }

    public class RateLimitOptions
    {
        public int Limit { get; set; }
        public TimeSpan Interval { get; set; }
    }
}
