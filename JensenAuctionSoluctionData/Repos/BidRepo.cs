using AutoMapper;
using Dapper;
using JensenAuctionSoluctionData.DataModels;
using JensenAuctionSoluctionData.Interfaces;
using JensenAuctionSolutionCommonInterfaces.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSoluctionData.Repos
{
    public class BidRepo : IBidRepo
    {
        private readonly DapperContext _dapperContext;
        private readonly IMapper _mapper;
        private readonly ILogger<BidRepo> _logger;
        private readonly IConfiguration _configuration;

        public BidRepo(DapperContext dapperContext, IMapper mapper, ILogger<BidRepo> logger, IConfiguration configuration)
        {
            _dapperContext = dapperContext;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }



        public async Task<AuctionDetailsInfoDto> GetAuctionDetailsWithWinningBidAsync(int auctionId)
        {
            try
            {
                using (var connection = _dapperContext.GetDbConnection())
                {
                    connection.Open();

                    var result = await connection.QueryMultipleAsync("GetAuctionDetailsWithWinningBid", new { AuctionId = auctionId }, commandType: CommandType.StoredProcedure);

                    // Read auction details
                    var auctionDetails = (await result.ReadAsync<AuctionDetailsInfoDto>()).FirstOrDefault();

                    if (auctionDetails != null)
                    {
                        if (await result.ReadAsync<BidInfoDto>() is var bidInfoDtos && bidInfoDtos.Any())
                        {
                            // Auction is open, read all bids
                            auctionDetails.Bids = bidInfoDtos.ToList();
                        }
                        else
                        {
                            // Auction is closed, read only the highest bid
                            var highestBid = (await result.ReadAsync<BidInfoDto>()).FirstOrDefault();
                            auctionDetails.Bids = highestBid != null ? new List<BidInfoDto> { highestBid } : new List<BidInfoDto>();
                        }
                    }

                    // Return auction details before leaving the using block
                    return auctionDetails;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving auction details.");
                throw;
            }
        }


        public async Task<IEnumerable<BidDetailsDto>> GetBidsForAuctionAsync(int auctionId)
        {
            try
            {
                using (var connection = _dapperContext.GetDbConnection())
                {
                    connection.Open();

                    // Execute the stored procedure
                    var bids = await connection.QueryAsync<BidDetailsDto>(
                        "GetBidsForAuction", // Replace with your actual stored procedure name
                        new { AuctionId = auctionId },
                        commandType: CommandType.StoredProcedure
                    );

                    return bids;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetBidsForAuctionAsync: {ex.Message}");
                throw; // Rethrow the exception to the calling code
            }
        }

        public async Task InsertBidAsync(IDbConnection connection, int auctionId, int userId, decimal price)
        {
            await connection.ExecuteAsync(
               "sp_InsertBid",
               new { AuctionId = auctionId, UserId = userId, Price = price },
               commandType: CommandType.StoredProcedure
           );
        }

        public async Task<bool> IsAuctionOpenAsync(IDbConnection connection, int auctionId)
        {
            var isOpen = await connection.QueryFirstOrDefaultAsync<bool>(
                "sp_CheckIfAuctionOpen",
                new { AuctionId = auctionId },
                commandType: CommandType.StoredProcedure
            );

            return isOpen;
        }

        public async Task<bool> IsBidHigherThanCurrentHighestBidAsync(IDbConnection connection, int auctionId, decimal price)
        {
            var maxBid = await connection.ExecuteScalarAsync<decimal?>(
                "sp_CheckIfBidHigherThanCurrentHighestBid",
                new { AuctionId = auctionId },
                commandType: CommandType.StoredProcedure
            );

            return price > (maxBid ?? 0);
        }

        //public async Task<bool> PlaceBidAsync(int auctionId, decimal price, int userid)
        //{
        //    using (var connection = _dapperContext.GetDbConnection())
        //    {
        //        try
        //        {
        //            connection.Open();

        //            var parameters = new DynamicParameters();
        //            parameters.Add("@AuctionId", auctionId, DbType.Int32);
        //            parameters.Add("@UserId", userid, DbType.Int32);
        //            parameters.Add("@Price", price, DbType.Decimal, precision: 18, scale: 2);

        //            // Execute the stored procedure
        //            await connection.ExecuteAsync("PlaceBid", parameters, commandType: CommandType.StoredProcedure);

        //            // If the execution reaches here, the bid placement was successful
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error placing bid");
        //            // Log the error or handle it based on your application's requirements
        //            return false; // Return false to indicate that bid placement failed
        //        }
        //    }
        //}
        public async Task<bool> PlaceBidAsync(int auctionId, decimal price, int userId)
        {
            using (var connection = _dapperContext.GetDbConnection())
            {
                try
                {
                    connection.Open();

                    if (await UserCreatedAuctionAsync(connection, auctionId, userId))
                    {
                        _logger.LogError("The user who created the auction cannot place bids on it.");
                        return false;
                    }

                    if (!await IsAuctionOpenAsync(connection, auctionId))
                    {
                        _logger.LogError("The auction is not open for bidding.");
                        return false;
                    }

                    if (!await IsBidHigherThanCurrentHighestBidAsync(connection, auctionId, price))
                    {
                        _logger.LogError("The bid is too low. It must be higher than the previous highest bid.");
                        return false;
                    }

                    await InsertBidAsync(connection, auctionId, userId, price);

                    return true; // Bid placement was successful
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error placing bid");
                    // Log the error or handle it based on your application's requirements
                    return false; // Return false to indicate that bid placement failed
                }
            }
        }


        public async Task<bool> RemoveBidAsync(int auctionId, int userId)
        {
            try
            {
                using (var connection = _dapperContext.GetDbConnection())
                {
                    connection.Open();

                    // Execute the stored procedure
                    await connection.ExecuteAsync(
                        "RemoveBid", // Replace with your actual stored procedure name
                        new { AuctionId = auctionId, UserId = userId },
                        commandType: CommandType.StoredProcedure
                    );
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in RemoveBidAsync: {ex.Message}");
                throw; // Rethrow the exception to the calling code
            }
        }

        public async Task<bool> UserCreatedAuctionAsync(IDbConnection connection, int auctionId, int userId)
        {
            var auctionCreatorId = await connection.ExecuteScalarAsync<int>(
                "sp_CheckIfUserCreatedAuction",
                new { AuctionId = auctionId },
                commandType: CommandType.StoredProcedure
            );

            return userId == auctionCreatorId;
        }


    }
}
