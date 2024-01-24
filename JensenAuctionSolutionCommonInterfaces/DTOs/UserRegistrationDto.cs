using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionCommonInterfaces.DTOs
{
    public class UserRegistrationDto
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}
