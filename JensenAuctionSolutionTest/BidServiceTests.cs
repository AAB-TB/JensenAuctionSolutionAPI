using AutoMapper;
using JensenAuctionSoluctionData.DataModels;
using JensenAuctionSoluctionData.Interfaces;
using JensenAuctionSoluctionData.Repos;
using JensenAuctionSolutionCore.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionTest
{
    public class BidServiceTests
    {
        [Fact]
        public async Task PlaceBidAsync_UserCreatedAuction_ReturnsFalse()
        {
            // Arrange
            var auctionId = 1;
            var price = 150.0m;
            var userId = 2;

            var mockBidRepo = new Mock<IBidRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<BidService>>();



            // Set up mock behavior for the business logic


            mockBidRepo.Setup(repo => repo.UserCreatedAuctionAsync(It.IsAny<IDbConnection>(), auctionId, userId))
                          .ReturnsAsync(true);

            var bidService = new BidService(mockBidRepo.Object, mockMapper.Object, mockLogger.Object);



            // Act
            var result = await bidService.PlaceBidAsync(auctionId, price, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PlaceBidAsync_AuctionNotOpen_ReturnsFalse()
        {
            // Arrange
            var auctionId = 1;
            var price = 150.0m;
            var userId = 2;


            var mockBidRepo = new Mock<IBidRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<BidService>>();


            // Set up mock behavior for the business logic
            mockBidRepo.Setup(repo => repo.UserCreatedAuctionAsync(It.IsAny<IDbConnection>(), auctionId, userId))
                       .ReturnsAsync(false);
            mockBidRepo.Setup(repo => repo.IsAuctionOpenAsync(It.IsAny<IDbConnection>(), auctionId))
                       .ReturnsAsync(false);

            var bidService = new BidService(mockBidRepo.Object, mockMapper.Object, mockLogger.Object);

            // Act
            var result = await bidService.PlaceBidAsync(auctionId, price, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PlaceBidAsync_BidLowerThanCurrentHighestBid_ReturnsFalse()
        {
            // Arrange
            var auctionId = 1;
            var price = 100.0m;
            var userId = 2;

            var mockBidRepo = new Mock<IBidRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<BidService>>();

            // Set up mock behavior for the business logic
            mockBidRepo.Setup(repo => repo.UserCreatedAuctionAsync(It.IsAny<IDbConnection>(), auctionId, userId))
                       .ReturnsAsync(false);
            mockBidRepo.Setup(repo => repo.IsAuctionOpenAsync(It.IsAny<IDbConnection>(), auctionId))
                       .ReturnsAsync(true);
            mockBidRepo.Setup(repo => repo.IsBidHigherThanCurrentHighestBidAsync(It.IsAny<IDbConnection>(), auctionId, price))
                       .ReturnsAsync(false);

            var bidService = new BidService(mockBidRepo.Object, mockMapper.Object, mockLogger.Object);

            // Act
            var result = await bidService.PlaceBidAsync(auctionId, price, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PlaceBidAsync_UnSuccessfulBid_Returnsfalse()
        {
            // Arrange
            var auctionId = 1;
            var price = 150.0m;
            var userId = 2;

            var mockBidRepo = new Mock<IBidRepo>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<BidService>>();

            // Set up mock behavior for the business logic
            mockBidRepo.Setup(repo => repo.UserCreatedAuctionAsync(It.IsAny<IDbConnection>(), auctionId, userId))
                       .ReturnsAsync(false);
            mockBidRepo.Setup(repo => repo.IsAuctionOpenAsync(It.IsAny<IDbConnection>(), auctionId))
                       .ReturnsAsync(true);
            mockBidRepo.Setup(repo => repo.IsBidHigherThanCurrentHighestBidAsync(It.IsAny<IDbConnection>(), auctionId, price))
                       .ReturnsAsync(true);
            mockBidRepo.Setup(repo => repo.InsertBidAsync(It.IsAny<IDbConnection>(), auctionId, userId, price))
                       .Returns(Task.CompletedTask);

            var bidService = new BidService(mockBidRepo.Object, mockMapper.Object, mockLogger.Object);

            // Act
            var result = await bidService.PlaceBidAsync(auctionId, price, userId);

            // Assert
            Assert.False(result);
        }
    }


}
