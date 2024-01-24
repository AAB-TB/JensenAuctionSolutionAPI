using AutoMapper;
using Dapper;
using JensenAuctionSoluctionData.DataModels;
using JensenAuctionSoluctionData.Interfaces;
using JensenAuctionSolutionCommonInterfaces.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using JensenAuctionSolutionDomain.Models;

namespace JensenAuctionSoluctionData.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly DapperContext _dapperContext;
        private readonly IMapper _mapper;
        private readonly ILogger<UserRepo> _logger;
        private readonly IConfiguration _configuration;

        public UserRepo(DapperContext dapperContext, IMapper mapper, ILogger<UserRepo> logger, IConfiguration configuration)
        {
            _dapperContext = dapperContext;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IEnumerable<UserInfoDto>> GetAllUsersAsync()
        {
            try
            {
                using (IDbConnection dbConnection = _dapperContext.GetDbConnection())
                {
                    dbConnection.Open();

                    // Execute the stored procedure
                    var users = await dbConnection.QueryAsync<UserInfoDto>("sp_GetAllUsers", commandType: CommandType.StoredProcedure);

                    return users;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all users: {ex.Message}");
                throw; // Handle the exception based on your application's requirements
            }
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    stringBuilder.Append(bytes[i].ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }


        public async Task<string> LoginUserAsync(string username, string password)
        {
            try
            {
                // Hash the provided password
                var hashedPassword = HashPassword(password);

                // Use Dapper connection for SQL query
                using (IDbConnection dbConnection = _dapperContext.GetDbConnection())
                {
                    dbConnection.Open();

                    // Create DynamicParameters
                    var parameters = new DynamicParameters();
                    parameters.Add("Username", username);
                    parameters.Add("PasswordHash", hashedPassword);
                    parameters.Add("UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);


                    // Execute the stored procedure
                    await dbConnection.ExecuteAsync("sp_LoginUser", parameters, commandType: CommandType.StoredProcedure);

                    // Retrieve output parameters

                    int userId = parameters.Get<int>("UserId");




                    if (userId == 0)
                    {
                        _logger.LogWarning("Invalid username or password.");
                        return null; // Invalid credentials
                    }

                    // User is valid, generate a token
                    var token = GenerateToken(userId);

                    return token;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error authenticating user: {ex.Message}");
                throw; // Handle the exception based on your application's requirements
            }
        }

        public async Task<bool> UserRegistrationAsync(UserRegistrationDto registrationDto)
        {
            try
            {
                using (IDbConnection dbConnection = _dapperContext.GetDbConnection())
                {
                    dbConnection.Open();

                    // Hash the password before storing it
                    string hashedPassword = HashPassword(registrationDto.PasswordHash);

                    // Create DynamicParameters
                    var parameters = new DynamicParameters();
                    parameters.Add("Username", registrationDto.Username);
                    parameters.Add("PasswordHash", hashedPassword);

                    // Execute the stored procedure
                    var affectedRows = await dbConnection.ExecuteAsync("sp_RegisterUser", parameters, commandType: CommandType.StoredProcedure);

                    // Log successful registration
                    _logger.LogInformation($"User '{registrationDto.Username}' registered successfully.");

                    // Return true if at least one row was affected (registration successful)
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions
                _logger.LogError($"Error during user registration: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UserUpdateAsync(int userId, UpdateUserDto updateUserDto, string oldPassword)
        {
            try
            {
                using (IDbConnection dbConnection = _dapperContext.GetDbConnection())
                {
                    dbConnection.Open();

                    // Verify old password directly in the SQL query
                    var user = await dbConnection.QueryFirstOrDefaultAsync<User>(
                        "SELECT * FROM Users WHERE UserId = @UserId AND PasswordHash = @OldPassword",
                        new { UserId = userId, OldPassword = HashPassword(oldPassword) });

                    if (user == null)
                    {
                        _logger.LogWarning("Invalid old password or user not found.");
                        return false; // Invalid old password or user not found
                    }

                    // Update user information
                    user.Username = updateUserDto.Username;
                    user.PasswordHash = HashPassword(updateUserDto.NewPassword); // Hash the new password

                    // Create DynamicParameters
                    var parameters = new DynamicParameters();
                    parameters.Add("UserId", user.UserId);
                    parameters.Add("Username", user.Username);
                    parameters.Add("PasswordHash", user.PasswordHash);

                    // Execute the stored procedure
                    var affectedRows = await dbConnection.ExecuteAsync("sp_UpdateUser", parameters, commandType: CommandType.StoredProcedure);

                    // Check if the update was successful
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user: {ex.Message}");
                throw; // Handle the exception based on your application's requirements
            }
        }


        private string GenerateToken(int userId)
        {
            var secretKey = _configuration["Jwt:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        
       
        // Add any additional claims as needed
    };

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Set expiration to 60 minutes
            var expires = DateTime.UtcNow.AddMinutes(60);

            var token = new JwtSecurityToken(
                issuer: "https://localhost:44393", // Set issuer
                audience: "https://localhost:44393", // Set audience
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            // Remove double quotes from the token string
            tokenString = tokenString.Replace("\"", string.Empty);

            return tokenString;
        }
    }
}
