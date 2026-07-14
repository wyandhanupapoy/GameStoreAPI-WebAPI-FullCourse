using GameStore.Api.Data;
using GameStore.Api.Repositories;
using GameStore.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using GameStore.Api.Middleware;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.OpenApi;
var builder = WebApplication.CreateBuilder(args);

// 1. Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
});

// 2. Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 3. Add Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add services to the container.
builder.Services.AddValidation();

// Database connection
var connString = builder.Configuration.GetConnectionString("GameStore");
builder.Services.AddDbContext<GameStoreContext>(options =>
    options.UseNpgsql(connString));

// Add Repositories and Services
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add Controllers
builder.Services.AddControllers();

// 4. Add OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// 5. Add Authentication (Production Ready)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing."))),
            
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Strict expiration
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(); // Use Global Exception Handler

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseRateLimiter(); // Use Rate Limiting
app.UseCors("AllowAll"); // Use CORS

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// NOTE: Auto-migration is removed for production readiness.
// Run 'dotnet ef database update' manually to apply migrations.

app.Run();