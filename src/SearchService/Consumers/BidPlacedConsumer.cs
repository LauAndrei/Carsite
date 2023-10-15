using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming Bid Placed");

        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        ArgumentNullException.ThrowIfNull(auction);

        // we're doing this inside the SearchService to show when searching for bids also the highest bid
        if (context.Message.BidStatus.Contains("Accepted")
            && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
        }
    }
}