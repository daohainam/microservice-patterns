using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Text;
using WebHook.DeliveryService.Infrastructure.Data;
using WebHook.DeliveryService.Infrastructure.Entity;

namespace WebHook.DeliveryService.DispatchService;

public class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger) : BackgroundService
{
    private static readonly HttpClient httpClient = new(new HttpClientHandler
    {
        // ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        httpClient.DefaultRequestHeaders.Add("X-Delivery-Service", "Microservice-Patterns-DeliveryService");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var loggerScope = logger.BeginScope("DispatchService");
            var nothingToProcess = true;

            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DeliveryServiceDbContext>();

                var queueItems = await dbContext.QueueItems.Include(d => d.WebHookSubscription)
                    .Where(d => d.ScheduledAt <= DateTime.UtcNow && !d.IsProcessed && d.RetryTimes < 5)
                    .OrderBy(d => d.ScheduledAt)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var item in queueItems)
                {
                    nothingToProcess = false;

                    logger.LogInformation("Processing item with ID: {id}", item.Id);

                    var result = await CallWebHookAsync(item, stoppingToken);

                    if (result.IsSuccess)
                    {
                        item.IsProcessed = true;
                        item.IsSuccess = true;
                        item.ErrorMessage = string.Empty;
                    }
                    else
                    {
                        item.IsSuccess = false;
                        item.ErrorMessage = result.ErrorMessage ?? string.Empty;
                        item.RetryTimes++;
                        item.ScheduledAt = DateTime.UtcNow.AddSeconds(30 * item.RetryTimes);
                    }

                    dbContext.QueueItems.Update(item);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing the queue items.");
            }

            if (nothingToProcess)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task<CallWebHookResult> CallWebHookAsync(DeliveryEventQueueItem item, CancellationToken cancellationToken)
    {
        var url = item.WebHookSubscription.Url;
        logger.LogInformation("Calling webhook at {url} with message: {message}", url, item.Message);

        if (IsSimulatedUrl(url))
        {
            return new CallWebHookResult
            {
                IsSuccess = true,
                ErrorMessage = string.Empty
            };
        }

        try
        {
            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Type = item.MessageType,
                Source = new Uri(item.MessageSource),
                DataContentType = MediaTypeNames.Application.Json,
                Data = item.Message
            };

            var content = cloudEvent.ToHttpContent(ContentMode.Structured, new JsonEventFormatter());

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers =
                {
                    { "X-Key", item.WebHookSubscription.SecretKey}
                },
                Content = content
            };
            var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new CallWebHookResult
                {
                    IsSuccess = true,
                    ErrorMessage = null
                };
            }
            else
            {
                return new CallWebHookResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error: {response.StatusCode} - {response.ReasonPhrase}"
                };
            }
        }
        catch (HttpRequestException ex)
        {
            return new CallWebHookResult
            {
                IsSuccess = false,
                ErrorMessage = $"Request error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new CallWebHookResult
            {
                IsSuccess = false,
                ErrorMessage = $"Unexpected error: {ex.Message}"
            };
        }
    }

    private static bool IsSimulatedUrl(string url)
    {
        return "http://localhost/webhook".Equals(url, StringComparison.OrdinalIgnoreCase);
    }
}

internal record CallWebHookResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}
