﻿using AutoMapper;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.Mappings
{
    public class UserMapper : Profile 
    {
        public UserMapper()
        {
            CreateMap<User, UserAdminResponse>();
            CreateMap<User, UserResponse>();
        }
    }
}
