using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[Route("api/auctions")]
[ApiController]
public class AuctionController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
            query = query.Where(x => x.UpdatedAt
                .CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);

        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .Where(a => a.Id == id)
            .Select(a => _mapper.Map<AuctionDto>(a))
            .FirstOrDefaultAsync();

        if (auction is null) return NotFound();

        return auction;
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction([FromBody] CreateAuctionDto request)
    {
        var auction = _mapper.Map<Auction>(request);

        // TODO: Add current user as seller
        auction.Seller = "Test";

        await _context.Auctions.AddAsync(auction);

        var newAuction = _mapper.Map<AuctionDto>(auction);

        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        // now these, (the upper 3 lines of code) are going to be treated like a transaction, and either they
        // all work, or none of them work. Because we're using entity framework, now publish method is part of the
        // same transaction that we're saving to the db down below

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to db");

        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto request)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction is null) return NotFound();

        // TODO: check seller name matches owner name

        auction.Item.Make = request.Make ?? auction.Item.Make;
        auction.Item.Model = request.Model ?? auction.Item.Model;
        auction.Item.Color = request.Color ?? auction.Item.Color;
        auction.Item.Mileage = request.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = request.Year ?? auction.Item.Year;

        // is before saveChanges so if our service bus was down, then this would still work and it would
        // save that message into the outbox in our database and when it comes back up it would be published
        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Problem saving changes");

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction is null) return NotFound();

        //TODO: check seller == username

        _context.Auctions.Remove(auction);

        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not delete");

        return Ok();
    }
}