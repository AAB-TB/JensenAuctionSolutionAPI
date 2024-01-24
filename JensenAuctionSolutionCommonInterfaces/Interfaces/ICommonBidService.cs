using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionCommonInterfaces.Interfaces
{
    public interface ICommonBidService
    {
        Task<IEnumerable<BidDetailsDto>> GetBidsForAuctionAsync(int auctionId);

        Task<bool> PlaceBidAsync(int auctionId, decimal price, int userid);

        Task<bool> RemoveBidAsync(int auctionId, int userId);

        Task<AuctionDetailsInfoDto> GetAuctionDetailsWithWinningBidAsync(int auctionId);
    }
}
