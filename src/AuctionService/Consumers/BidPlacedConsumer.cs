using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly AuctionDbContext _dbContext;

    public BidPlacedConsumer(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming BidPlaced");

        var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

        ArgumentNullException.ThrowIfNull(auction);

        // when we get this events, we are not guaranteed that they're gonna come in any specific order,
        // so we can alleviate that particular problem by just checking what the current high bid is;
        // if this bid is higher than that current high bid, then we update that particular property
        if (auction.CurrentHighBid is null
            || context.Message.BidStatus.Contains("Accepted")
            && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await _dbContext.SaveChangesAsync();
        }
    }
}