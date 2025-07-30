using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using taskflow_api.TaskFlow.API.Hubs;
using taskflow_api.TaskFlow.Application.DTOs.Common;
using taskflow_api.TaskFlow.Application.DTOs.Common.Attributes;
using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Application.Mappings;
using taskflow_api.TaskFlow.Application.Services;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Data;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Infrastructure.Repository;
using taskflow_api.TaskFlow.Shared.Exceptions;
using taskflow_api.TaskFlow.Shared.Helpers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHostedService<DeadlineNotificationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<ISprintService, SprintService>();
builder.Services.AddScoped<ITaskFlowAuthorizationService, TaskFlowAuthorizationService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ITaskProjectService, TaskProjectService>();
builder.Services.AddScoped<ITaskCommentService, TaskCommentService>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<ITermService, TermService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IProjectPartService, ProjectPartService>();
builder.Services.AddHttpClient<IGitHubRepoService, GitHubRepoService>();
builder.Services.AddScoped<ICodeScanService, SonarScannerService>();
builder.Services.AddScoped<ISprintMeetingLogsService, SprintMeetingLogsService>();

//Repository
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IVerifyTokenRopository, VerifyTokenRopository>();
builder.Services.AddScoped<ISprintRepository, SprintRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITaskTagRepository, TaskTagRepository>();
builder.Services.AddScoped<ITaskProjectRepository, TaskProjectRepository>();
builder.Services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();
builder.Services.AddScoped<ITaskAssigneeRepository, TaskAssigneeRepository>();
builder.Services.AddScoped<ITaskIssueRepository, TaskIssueRepository>();
builder.Services.AddScoped<ITermRepository, TermRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IProjectPartRepository, ProjectPartRepository>();
builder.Services.AddScoped<IUserIntegrationRepository, UserIntegrationRepository>();
builder.Services.AddScoped<ICommitRecordRepository, CommitRecordRepository>();
builder.Services.AddScoped<IRabbitMQService, RabbitMQService>();
builder.Services.AddScoped<IUserGitHubRepository, UserGitHubRepository>();
builder.Services.AddScoped<ICommitScanIssueRepository, CommitScanIssueRepository>();
builder.Services.AddScoped<IGitMemberRepository, GitMemberRepository>();
builder.Services.AddScoped<IGitHubMemberService, GitHubMemberService>();
builder.Services.AddScoped<ISprintMeetingLogsRepository, SprintMeetingLogsRepository>();

//Signalr
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();


// Mapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<ITaskFlowAuthorizationService, TaskFlowAuthorizationService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskFlow API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
    //add enum
    c.SchemaFilter<EnumSchemaFilter>();
});

//set time
builder.Services.Configure<TimeSettings>(builder.Configuration.GetSection("TimeSever"));
builder.Services.AddSingleton<AppTimeProvider>();


builder.Services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbApp")));

// services Cloudinary
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSingleton(s =>
{
    var config = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
    var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
    return new Cloudinary(account);
});

//SonarQube
builder.Services.AddHostedService<RabbitScanCodeConsumerHostedService>();
builder.Services.Configure<SonarQubeSetting>(
    builder.Configuration.GetSection("SonarQube"));

//RabbitMQ
builder.Services.Configure<RabbitMQSetting>(builder.Configuration.GetSection("RabbitMQ"));

//GitHub login 
//builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


// Override default 400 validation error response to return custom ApiResponse format
// This ensures all model validation errors are consistently returned in ApiResponse<T> structure
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

        var result = ApiResponse<string>.Error(1000, string.Join("; ", errors));

        return new BadRequestObjectResult(result);
    };
});

// Configure CORS policy
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFE", policy =>
    {
        policy.WithOrigins(allowedOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure Identity
builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<TaskFlowDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Configure Gmail
builder.Services.Configure<MailSettings>(
    builder.Configuration.GetSection("MailSetting"));

builder.Services.Configure<AppSetting>(
    builder.Configuration.GetSection("AppSetting"));

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters
        = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateIssuer = true,
            ClockSkew = TimeSpan.Zero,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
            ValidAudience = builder.Configuration["Jwt:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    options.Events = new JwtBearerEvents
    {
        // Throwing AppException directly inside OnAuthenticationFailed event will NOT work as expected.
        // The JwtBearer middleware does NOT catch exceptions thrown here, so the exception may be swallowed or cause unexpected behavior,
        // and no proper JSON error response will be sent to the client.
        // Therefore, we must handle the error by setting the response status code and writing the JSON error body directly in this event handler.
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var error = new
            {
                Code = ErrorCode.Unauthorized.Code,
                Message = ErrorCode.Unauthorized.Message,
                Status = ErrorCode.Unauthorized.StatusCode
            };
            var json = JsonSerializer.Serialize(error);

            return context.Response.WriteAsync(json);
        },
    };
});

builder.Services.AddMemoryCache();
var env = builder.Environment;
var certPath = builder.Configuration["Kestrel:Certificates:Default:Path"];
var certPassword = builder.Configuration["Kestrel:Certificates:Default:Password"];

//builder.WebHost.UseUrls("https://*:7029");
//builder.WebHost.ConfigureKestrel(options =>
//{
//    if (env.IsDevelopment())
//    {
//        try
//        {
//            options.ListenLocalhost(7029, lo => lo.UseHttps());
//        }
//        catch
//        {
//            options.ListenLocalhost(5080);
//            Console.WriteLine("[Dev] No dev-certs found. Serving HTTP on http://localhost:5080");
//        }
//    }
//    else
//    {
//        if (!string.IsNullOrWhiteSpace(certPath))
//        {
//            options.ListenAnyIP(7029, lo => lo.UseHttps(certPath, certPassword)); // HTTPS
//        }
//        else
//        {
//            options.ListenAnyIP(8080); // fallback HTTP
//            Console.WriteLine("[Prod] No PFX configured. Serving HTTP on :8080. Set Kestrel:Certificates:Default:* to enable HTTPS.");
//        }
//    }
//});


builder.Logging.AddConsole();
var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

//create account admin if not exists
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<User>>();

    if (!userManager.Users.Any())
    {
        // Create a default system user if no users exist
        var system = new User
        {
            Id = new Guid("00000000-0000-0000-0000-000000000002"),
            UserName = "system",
            Email = "system@taskflow.com",
            EmailConfirmed = true,
            FullName = "System",
            Role = UserRole.Admin,
            IsActive = true
        };
        var resultsy = await userManager.CreateAsync(system, "system123456");

        // Create a default admin user if no users exist
        var admin = new User
        {
            Id = new Guid("00000000-0000-0000-0000-000000000001"),
            UserName = "admin",
            Email = "admin@taskflow.com",
            EmailConfirmed = true,
            FullName = "Admin",
            Role = UserRole.Admin,
            IsActive = true
        };
        var resultad = await userManager.CreateAsync(admin, "admin123456");

        if (!resultsy.Succeeded || !resultad.Succeeded)
        {
            foreach (var error in resultsy.Errors)
            {
                Console.WriteLine($"Error creating default SYSTEM user: {error.Description}");
            }
            foreach (var error in resultad.Errors)
            {
                Console.WriteLine($"Error creating default ADMIN user: {error.Description}");
            }
        }
        else
        {
            Console.WriteLine("Default admin user created successfully.");
        }
    }
}

    // Configure the HTTP request pipeline.
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.

//swager
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API V1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at the app's root
    });

//app.UseHttpsRedirection();

app.UseCors("AllowFE");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

//Map hub
app.MapHub<TaskHub>("/taskHub");
app.Run();