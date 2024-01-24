using AutoMapper;
using JensenAuctionSoluctionData.Interfaces;
using JensenAuctionSoluctionData.Repos;
using JensenAuctionSolutionCommonInterfaces.DTOs;
using JensenAuctionSolutionCore.Interfaces;
using JensenAuctionSolutionDomain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionCore.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionRepo _auctionRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<AuctionService> _logger;

        public AuctionService(IAuctionRepo auctionRepo, IMapper mapper, ILogger<AuctionService> logger)
        {
            _auctionRepo = auctionRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Auction> CreateAuctionAsync(int userid, AuctionDto auctionDto)
        {
            return await _auctionRepo.CreateAuctionAsync(userid, auctionDto);
        }

        public async Task<IEnumerable<AuctionInfoDto>> GetAllAuctionsAsync()
        {
            return await _auctionRepo.GetAllAuctionsAsync();
        }

        public async Task<AuctionDetailsInfoDto> GetAuctionDetailsAsync(int auctionId)
        {
            return await _auctionRepo.GetAuctionDetailsAsync(auctionId);
        }

        public async Task<bool> RemoveAuctionAsync(int auctionId)
        {
            return await _auctionRepo.RemoveAuctionAsync(auctionId);
        }

        public async Task<IEnumerable<AuctionDetailsInfoDto>> SearchAuctionsAsync(string searchQuery)
        {
            return await _auctionRepo.SearchAuctionsAsync(searchQuery);
        }

        public async Task<AuctionDetailsInfoDto> UpdateAuctionAsync(int UserId, int auctionId, AuctionDto auctionDto)
        {
            return await _auctionRepo.UpdateAuctionAsync(UserId, auctionId, auctionDto);
        }
    }
}
