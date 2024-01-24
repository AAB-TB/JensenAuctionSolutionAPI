using JensenAuctionSolutionCommonInterfaces.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionCommonInterfaces.Interfaces
{
    public interface ICommonUserService
    {
        Task<bool> UserRegistrationAsync(UserRegistrationDto registrationDto);
        Task<bool> UserUpdateAsync(int userId, UpdateUserDto updateUserDto, string oldPassword);
        Task<IEnumerable<UserInfoDto>> GetAllUsersAsync();
        Task<string> LoginUserAsync(string username, string password);
    }
}
