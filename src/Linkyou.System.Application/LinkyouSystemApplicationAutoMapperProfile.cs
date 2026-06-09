using AutoMapper;
using Linkyou.System.Menus;

namespace Linkyou.System;

public class LinkyouSystemApplicationAutoMapperProfile : Profile
{
    public LinkyouSystemApplicationAutoMapperProfile()
    {
        CreateMap<Menu, MenuDto>();
    }
}
