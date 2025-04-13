using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using WebHook.DeliveryService.Infrastructure.Data;

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

                    var result = await CallWebHookAsync(item.WebHookSubscription.Url, item.WebHookSubscription.SecretKey, item.Message, stoppingToken);

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

    private async Task<CallWebHookResult> CallWebHookAsync(string url, string secretKey, string message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Calling webhook at {url} with message: {message}", url, message);

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
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers =
                {
                    { "X-Key", secretKey }
                },
                Content = new StringContent(message, Encoding.UTF8, "application/json")
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
