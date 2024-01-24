using JensenAuctionSolutionCommonInterfaces.DTOs;
using JensenAuctionSolutionDomain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionCommonInterfaces.Interfaces
{
    public interface ICommonAuctionService
    {
        Task<Auction> CreateAuctionAsync(int userid, AuctionDto auctionDto);

        Task<AuctionDetailsInfoDto> UpdateAuctionAsync(int UserId, int auctionId, AuctionDto auctionDto);

        Task<bool> RemoveAuctionAsync(int auctionId);

        Task<IEnumerable<AuctionInfoDto>> GetAllAuctionsAsync();

        Task<AuctionDetailsInfoDto> GetAuctionDetailsAsync(int auctionId);

        Task<IEnumerable<AuctionDetailsInfoDto>> SearchAuctionsAsync(string searchQuery);

    }
}
