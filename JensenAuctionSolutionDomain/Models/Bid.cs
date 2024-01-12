using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionDomain.Models
{
    public class Bid
    {
        public int BidId { get; set; }
        public decimal Price { get; set; }

        
        public int AuctionId { get; set; }

        
        public Auction Auction { get; set; }

        
        public int UserId { get; set; }

        
        public User User { get; set; }
    }
}
