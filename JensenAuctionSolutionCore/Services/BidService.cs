using AutoMapper;
using JensenAuctionSoluctionData.Interfaces;
using JensenAuctionSolutionCommonInterfaces.DTOs;
using JensenAuctionSolutionCore.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionCore.Services
{
    public class BidService : IBidService
    {
        private readonly IBidRepo _bidRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<BidService> _logger;

        public BidService(IBidRepo bidRepo, IMapper mapper, ILogger<BidService> logger)
        {
            _bidRepo = bidRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuctionDetailsInfoDto> GetAuctionDetailsWithWinningBidAsync(int auctionId)
        {
            return await _bidRepo.GetAuctionDetailsWithWinningBidAsync(auctionId);
        }

        public async Task<IEnumerable<BidDetailsDto>> GetBidsForAuctionAsync(int auctionId)
        {
            return await _bidRepo.GetBidsForAuctionAsync(auctionId);
        }

        public async Task<bool> PlaceBidAsync(int auctionId, decimal price, int userid)
        {
            return await _bidRepo.PlaceBidAsync(auctionId, price, userid);
        }

        public async Task<bool> RemoveBidAsync(int auctionId, int userId)
        {
            return await _bidRepo.RemoveBidAsync(auctionId, userId);
        }
    }
}
