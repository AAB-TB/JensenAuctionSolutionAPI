using JensenAuctionSolutionCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JensenAuctionSolutionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        private readonly IBidService _bidService;
        private readonly ILogger<BidController> _logger;
        public BidController(
            IBidService bidService,
            ILogger<BidController> logger
            )
        {
            _bidService = bidService;
            _logger = logger;
        }
        [HttpGet("{auctionId}")]
        [Authorize]
        public async Task<IActionResult> GetBidsForAuction(int auctionId)
        {
            try
            {
                var bids = await _bidService.GetBidsForAuctionAsync(auctionId);
                return Ok(bids);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetBidsForAuction: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("winning/{auctionId}")]

        public async Task<IActionResult> GetAuctionDetailsWithWinningBid(int auctionId)
        {
            try
            {
                var winningBid = await _bidService.GetAuctionDetailsWithWinningBidAsync(auctionId);

                if (winningBid != null)
                {
                    return Ok(winningBid);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetWinningBid: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceBid(int auctionId, decimal price)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("User not authenticated.");
                }
                var placedBid = await _bidService.PlaceBidAsync(auctionId, price, userId);

                if (placedBid != null)
                {
                    return Ok(placedBid);
                }
                else
                {
                    return BadRequest("Bid placement failed due to validation errors.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PlaceBid: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpDelete("{auctionId}/{userId}")]
        [Authorize]
        public async Task<IActionResult> RemoveBid(int auctionId, int userId)
        {
            try
            {
                await _bidService.RemoveBidAsync(auctionId, userId);
                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in RemoveBid: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
