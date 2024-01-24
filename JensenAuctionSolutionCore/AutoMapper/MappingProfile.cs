using AutoMapper;
using JensenAuctionSolutionCommonInterfaces.DTOs;
using JensenAuctionSolutionDomain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JensenAuctionSolutionCore.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //mapper.Map<DestinationType>(sourceObject)

            // Model to DTO mappings
            CreateMap<User, UserDto>();  //<source,destination>
            CreateMap<User, UserInfoDto>();
            CreateMap<Auction, AuctionDto>();
            CreateMap<Bid, BidDto>();
            CreateMap<Bid, BidInfoDto>();

            CreateMap<Auction, AuctionDetailsInfoDto>();
            CreateMap<Auction, AuctionInfoDto>().ReverseMap();

        }
    }
}
