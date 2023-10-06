using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        
        public AuctionController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
        {
            return await _context.Auctions
                .Include(a => a.Item)
                .OrderBy(a => a.Item.Make)
                .Select(a => _mapper.Map<AuctionDto>(a))
                .ToListAsync();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
                .Include(a => a.Item)
                .Where(a => a.Id == id)
                .Select(a => _mapper.Map<AuctionDto>(a))
                .FirstOrDefaultAsync();

            if (auction is null)
            {
                return NotFound();
            }

            return auction;
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction([FromBody] CreateAuctionDto request)
        {
            var auction = _mapper.Map<Auction>(request);

            // TODO: Add current user as seller
            auction.Seller = "Test";
            
            await _context.Auctions.AddAsync(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Could not save changes to db");
            }

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto request)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction is null)
            {
                return NotFound();
            }
            
            // TODO: check seller name matches owner name

            auction.Item.Make = request.Make ?? auction.Item.Make;
            auction.Item.Model = request.Model ?? auction.Item.Model;
            auction.Item.Color = request.Color ?? auction.Item.Color;
            auction.Item.Mileage = request.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = request.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Problem saving changes");
            }
            
            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction is null)
            {
                return NotFound();
            }
            
            //TODO: check seller == username

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Could not delete");
            }

            return Ok();
        }
        
    }
}
