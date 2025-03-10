using AutoMapper;
using InventoryManagement.Application.DTOs.Item;
using InventoryManagement.Application.DTOs.Location;
using InventoryManagement.Application.DTOs.Transaction;
using InventoryManagement.Application.DTOs.User;
using InventoryManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InventoryManagement.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Конфигурируем маппинг для всех сущностей и их DTO
            CreateMap<User, UserCreateDTO>().ReverseMap();
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
