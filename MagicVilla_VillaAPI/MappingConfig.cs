using AutoMapper;
using MagicVilla_VillaAPI.Dto;
using MagicVilla_VillaAPI.Models;

namespace MagicVilla_VillaAPI;

public class MappingConfig : Profile
{
    public MappingConfig()
    {
        CreateMap<Villa, VillaDTO>().ReverseMap();

        CreateMap<Villa,VillaCreateDTO>().ReverseMap();
        CreateMap<Villa,VillaUpdateDTO>().ReverseMap();
    }
}
