using Common.DTOs.Item;

namespace Armory.SyncDataServices;

public class GameItemsHttpService
{
    private readonly ILogger<GameItemsHttpService> _logger;
    private readonly HttpClient _httpClient;

    public GameItemsHttpService(
        ILogger<GameItemsHttpService> logger,
        IConfiguration configuration,
        HttpClient httpClient
    )
    {
        var baseUrl = configuration["GameService:BaseUrl"];

        if (baseUrl == null)
            throw new Exception($"Game service url not found on {AppDomain.CurrentDomain.FriendlyName} configurations");

        _logger = logger;

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<ItemPriceDto?> GetItemAsync(Guid itemTransactionId)
    {
        _logger.LogInformation(
            "Service {ServiceName} is doing a sync request (http) for item {ItemTransactionId}",
            AppDomain.CurrentDomain.FriendlyName,
            itemTransactionId
        );

        try
        {
            var response = await _httpClient.GetFromJsonAsync<ItemPriceDto>(
                requestUri: $"/item/{itemTransactionId}/price"
            );

            _logger.LogInformation(
                "Information about item {ItemTransactionId} was successfully retrieved",
                itemTransactionId
            );

            return response;
        }
        catch (Exception)
        {
            _logger.LogInformation(
                "Information about item {ItemTransactionId} could not be retrieved",
                itemTransactionId
            );

            return new ItemPriceDto
            {
                TransactionId = itemTransactionId,
                Price = 0,
            };
        }
    }
}
