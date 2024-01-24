using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionCommonInterfaces.DTOs
{
    public class AuctionDetailsInfoDto
    {
        public int AuctionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AuctionCreatorId { get; set; }
        public string AuctionCreator { get; set; }
        public List<BidInfoDto> Bids { get; set; }
    }
}
