using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Net;
using System.Net.Http;

namespace RetryPattern.API.Configuration
{
    public static class PollyConfig
    {
        public static IServiceCollection AddPollyConfiguration(this IServiceCollection services)
        {
            services.AddHttpClient("PollyTest", c =>
            {
                c.BaseAddress = new Uri("http://localhost:6000");
            })
            .AddPolicyHandler(policy => PollyExtensions.WaitAndRetry(policy));

            return services;
        }
    }

    public static class PollyExtensions
    {
        public static AsyncRetryPolicy<HttpResponseMessage> WaitAndRetry(HttpRequestMessage policy)
        {
            var retry = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                }, (message, timespan, retryCount, context) =>
                {
                    policy.Headers.Remove("Retry-Count");
                    policy.Headers.Add("Retry-Count", retryCount.ToString());

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Out.WriteLine($"Content: {message.Result.Content.ReadAsStringAsync().Result}");
                    Console.Out.WriteLine($"ReasonPhrase: {message.Result.ReasonPhrase}");
                    Console.Out.WriteLine($"Tentando pela {retryCount} vez!");
                    Console.ForegroundColor = ConsoleColor.White;
                });

            return retry;
        }

        public static AsyncRetryPolicy<HttpResponseMessage> WaitAndRetryHeaderSort(HttpRequestMessage policy)
        {
            var retry = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(3, retryCount =>
                {
                    policy.Headers.Remove("Retry-Count");
                    policy.Headers.Add("Retry-Count", retryCount.ToString());

                    return TimeSpan.FromSeconds(retryCount * 3);
                });

            return retry;
        }

    }
}