using AutoMapper;
using Dapper;
using JensenAuctionSoluctionData.DataModels;
using JensenAuctionSoluctionData.Interfaces;
using JensenAuctionSolutionCommonInterfaces.DTOs;
using JensenAuctionSolutionDomain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSoluctionData.Repos
{
    public class AuctionRepo : IAuctionRepo
    {
        private readonly DapperContext _dapperContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AuctionRepo> _logger;
        private readonly IConfiguration _configuration;

        public AuctionRepo(DapperContext dapperContext, IMapper mapper, ILogger<AuctionRepo> logger, IConfiguration configuration)
        {
            _dapperContext = dapperContext;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Auction> CreateAuctionAsync(int userid, AuctionDto auctionDto)
        {
            try
            {
                using (var connection = _dapperContext.GetDbConnection())
                {
                    connection.Open();

                    var parameters = new DynamicParameters();
                    parameters.Add("@Title", auctionDto.Title);
                    parameters.Add("@Description", auctionDto.Description);
                    parameters.Add("@Price", auctionDto.Price);
                    parameters.Add("@StartDate", auctionDto.StartDate);
                    parameters.Add("@EndDate", auctionDto.EndDate);
                    parameters.Add("@CurrentUser", userid); // Use the passed user ID
                    parameters.Add("@Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // Execute the stored procedure
                    await connection.ExecuteAsync(
                        "CreateAuction",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    // Check the success flag
                    if (!(bool)parameters.Get<bool>("@Success"))
                    {
                        // Handle validation failure (e.g., throw an exception, log the error)
                        _logger.LogError("Auction creation failed due to validation errors.");
                        return null;
                    }

                    // Success: Return the newly created auction
                    return await connection.QueryFirstOrDefaultAsync<Auction>(
                        "SELECT TOP 1 * FROM Auctions ORDER BY AuctionId DESC"
                    );
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                _logger.LogError($"Error in CreateAuctionAsync: {ex.Message}");
                throw; // Rethrow the exception to the calling code
            }
        }

        public async Task<IEnumerable<AuctionInfoDto>> GetAllAuctionsAsync()
        {
            try
            {
                using (var connection = _dapperContext.GetDbConnection())
                {
                    connection.Open();

                    // Execute the stored procedure
                    var auctions = await connection.QueryAsync<AuctionInfoDto>(
                        "GetAllAuctions", // Replace with your actual stored procedure name
                        commandType: CommandType.StoredProcedure
                    );
                    var auctionDtoList = _mapper.Map<IEnumerable<AuctionInfoDto>>(auctions);

                    return auctionDtoList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllAuctionsAsync: {ex.Message}");
                throw; // Rethrow the exception to the calling code
            }
        }

        public async Task<AuctionDetailsInfoDto> GetAuctionDetailsAsync(int auctionId)
        {
            try
            {
                using (var connection = _dapperContext.GetDbConnection())
                {
                    connection.Open();

                    var result = await connection.QueryMultipleAsync("GetAuctionDetails", new { AuctionId = auctionId }, commandType: CommandType.StoredProcedure);

                    // Read auction details
                    var auctionDetails = (await result.ReadAsync<AuctionDetailsInfoDto>()).FirstOrDefault();

                    if (auctionDetails != null)
                    {
                        // Read associated bids
                        auctionDetails.Bids = (await result.ReadAsync<BidInfoDto>()).ToList();
                    }

                    return auctionDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving auction details.");
                // You can rethrow the exception if needed or handle it according to your application's requirements.
                throw;
            }
        }

        public async Task<bool> RemoveAuctionAsync(int auctionId)
        {
            try
            {
                using (var connection = _dapperContext.GetDbConnection())
                {
                    connection.Open();

                    // Execute the stored procedure
                    await connection.ExecuteAsync(
                        "RemoveAuction", // Replace with your actual stored procedure name
                        new { AuctionId = auctionId },
                        commandType: CommandType.StoredProcedure
                    );
                    return true;
                }
            }
            catch (SqlException ex) when (ex.Number == 51003)
            {
                // Handle the specific case where the stored procedure throws an exception with error code 51003
                _logger.LogError($"Error in RemoveAuctionAsync: {ex.Message}");
                // You might want to log or handle this specific error differently
                throw new ApplicationException("Cannot delete auction with bids. Remove bids first.");
            }
            catch (Exception ex)
            {
                // Log and rethrow other exceptions
                _logger.LogError($"Error in RemoveAuctionAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<AuctionDetailsInfoDto>> SearchAuctionsAsync(string searchQuery)
        {
            try
            {
                using (var connection = _dapperContext.GetDbConnection())
                {
                    connection.Open();

                    // Execute the stored procedure with multiple result sets
                    var result = await connection.QueryMultipleAsync(
                        "SearchAuctions",
                        new { SearchQuery = searchQuery },
                        commandType: CommandType.StoredProcedure
                    );

                    // Read auction details
                    var auctions = (await result.ReadAsync<AuctionDetailsInfoDto>()).ToList();

                    // Read associated bids
                    foreach (var auction in auctions)
                    {
                        auction.Bids = (await result.ReadAsync<BidInfoDto>()).ToList();
                    }

                    return auctions;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SearchAuctionsAsync: {ex.Message}");
                throw; // Rethrow the exception to the calling code
            }
        }

        public async Task<AuctionDetailsInfoDto> UpdateAuctionAsync(int userId, int auctionId, AuctionDto auctionDto)
        {
            try
            {
                using (var connection = _dapperContext.GetDbConnection())
                {
                    connection.Open();

                    // Check if the user is not the auction creator
                    bool isAuctionCreator = await IsUserAuctionCreatorAsync(userId, auctionId);

                    if (isAuctionCreator)
                    {
                        // Execute the stored procedure
                        await connection.ExecuteAsync(
                            "UpdateAuction", // Replace with your actual stored procedure name
                            new
                            {
                                UserId = userId,
                                AuctionId = auctionId,
                                Title = auctionDto.Title,
                                Description = auctionDto.Description,
                                Price = auctionDto.Price,
                                StartDate = auctionDto.StartDate,
                                EndDate = auctionDto.EndDate
                            },
                            commandType: CommandType.StoredProcedure
                        );

                        // Retrieve and return the updated auction
                        return await GetAuctionDetailsAsync(auctionId);
                    }
                    else
                    {
                        _logger.LogInformation("User is the auction creator. No update performed.");
                        return null;  // You can return null or handle it based on your requirements
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateAuctionAsync: {ex.Message}");
                throw; // Rethrow the exception to the calling code
            }
        }

        // Helper method to check if the user is the auction creator
        private async Task<bool> IsUserAuctionCreatorAsync(int userId, int auctionId)
        {
            using (var connection = _dapperContext.GetDbConnection())
            {
                connection.Open();

                // Execute a query to check if the user is the auction creator
                var result = await connection.QueryFirstOrDefaultAsync<int>(
                    "CheckIfUserIsAuctionCreator",  // Replace with your actual stored procedure name
                    new { UserId = userId, AuctionId = auctionId },
                    commandType: CommandType.StoredProcedure
                );

                // Convert the result to a boolean (1 means true, 0 means false)
                return result == 1;
            }
        }

    }
}
