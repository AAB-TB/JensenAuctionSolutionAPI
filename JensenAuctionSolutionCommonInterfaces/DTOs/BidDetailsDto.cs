using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionCommonInterfaces.DTOs
{
    public class BidDetailsDto
    {

        public int AuctionId { get; set; }
        public string AuctionTitle { get; set; }
        public int BidderId { get; set; }
        public string BidderName { get; set; }
        public decimal Price { get; set; }
    }
}
