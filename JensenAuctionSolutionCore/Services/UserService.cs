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
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepo userRepo, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<UserInfoDto>> GetAllUsersAsync()
        {
            return await _userRepo.GetAllUsersAsync();
        }

        public async Task<string> LoginUserAsync(string username, string password)
        {
            return await _userRepo.LoginUserAsync(username, password);
        }

        public async Task<bool> UserRegistrationAsync(UserRegistrationDto registrationDto)
        {
            return await _userRepo.UserRegistrationAsync(registrationDto);
        }

        public async Task<bool> UserUpdateAsync(int userId, UpdateUserDto updateUserDto, string oldPassword)
        {
            return await _userRepo.UserUpdateAsync(userId, updateUserDto, oldPassword);
        }
    }
}
