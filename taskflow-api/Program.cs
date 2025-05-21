using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using taskflow_api.Data;
using taskflow_api.Entity;
using taskflow_api.Exceptions;
using taskflow_api.Model.Response;
using taskflow_api.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbApp")));

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
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure Identity
builder.Services.AddIdentityCore<User>(options =>
{
    //options.Password.RequireDigit = false;
    //options.Password.RequireLowercase = false; 
    //options.Password.RequireUppercase = false;
    //options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<TaskFlowDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
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
        = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
            ValidAudience = builder.Configuration["Jwt:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder
                    .Configuration["Jwt:Key"]!)),
        };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware< GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
