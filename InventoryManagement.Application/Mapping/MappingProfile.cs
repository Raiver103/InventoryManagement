using AutoMapper;
using InventoryManagement.Application.DTOs.Item;
using InventoryManagement.Application.DTOs.Location;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.DTOs.User;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Entities.Auth0;

namespace InventoryManagement.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, CreateUserRequest>().ReverseMap();
            CreateMap<User, UserResponseDTO>().ReverseMap(); 

            CreateMap<Location, LocationCreateDTO>().ReverseMap();
            CreateMap<Location, LocationResponseDTO>().ReverseMap(); 

            CreateMap<Item, ItemCreateDTO>().ReverseMap();
            CreateMap<Item, ItemResponseDTO>().ReverseMap();

            CreateMap<Transaction, TransactionCreateDTO>().ReverseMap();
            CreateMap<Transaction, TransactionResponseDTO>().ReverseMap();
        }
    }
}
