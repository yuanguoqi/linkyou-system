using AutoMapper;
using Linkyou.System.Menus;

namespace Linkyou.System;

/// <summary>
/// AutoMapper 映射配置
/// 在此文件中注册所有实体到 DTO 的映射关系
/// </summary>
public class LinkyouSystemApplicationAutoMapperProfile : Profile
{
    public LinkyouSystemApplicationAutoMapperProfile()
    {
        CreateMap<Menu, MenuDto>();
    }
}
