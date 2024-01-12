using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionDomain.Models
{
    public class Auction
    {
        public int AuctionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Foreign key for Users table
        public int UserId { get; set; }

        // Navigation property for User
        public User User { get; set; }

        // Navigation property for Bids
        public ICollection<Bid> Bids { get; set; }
    }
}
