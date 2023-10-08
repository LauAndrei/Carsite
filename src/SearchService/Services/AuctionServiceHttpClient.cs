using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionServiceHttpClient
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        // this is gonna give us the date of the auction that's been updated the latest in our db
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(item => item.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        return await _httpClient.GetFromJsonAsync<List<Item>>(_configuration["AuctionServiceUrl"] +
                                                              "/api/auctions?date=" + lastUpdated);
    }
}