﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Request;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.API.Controllers
{
    [Route("project")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _context;
        private readonly ITaskFlowAuthorizationService _authorization;

        public ProjectController(IProjectService context, ITaskFlowAuthorizationService authorization)
        {
            _context = context;
            _authorization = authorization;
        }

        [HttpPost]
        [Authorize]
        public async Task<ApiResponse<ProjectResponse>> CreateProject([FromBody] CreateProjectRequest request)
        {
            var project = await _context.CreateProject(request);
            return ApiResponse<ProjectResponse>.Success(project);
        }

        [HttpPut("{projectId}")]
        [Authorize]
        public async Task<ApiResponse<ProjectResponse>> UpdateProject([FromForm] UpdateProjectRequest request)
        {
            var isAuthorized = await _authorization.AuthorizeAsync(request.ProjectId, ProjectRole.Leader);
            if (!isAuthorized)
            {
                return ApiResponse<ProjectResponse>.Error(9002, "Unauthorized access");
            }
            var project = await _context.UpdateProject(request);
            return ApiResponse<ProjectResponse>.Success(project);
        }

        [HttpGet]
        [Authorize]
        public async Task<ApiResponse<List<ProjectsResponse>>> ListProjects()
        {
            var projects = await _context.ListProjectResponse();
            return ApiResponse<List<ProjectsResponse>>.Success(projects);
        }

        [HttpGet("{projectid}")]
        public async Task<ApiResponse<ProjectDetailResponse>> GetProject(Guid projectid)
        {
            var project = await _context.GetProject(projectid);
            return ApiResponse<ProjectDetailResponse>.Success(project);
        }
    }
}
