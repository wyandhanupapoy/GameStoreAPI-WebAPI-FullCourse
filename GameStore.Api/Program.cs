using GameStore.Api.Data;
using GameStore.Api.Repositories;
using GameStore.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.AddGameStoreDb();
builder.Services.AddControllers();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGenreService, GenreService>();

var app = builder.Build();

app.MapControllers();

app.MigrateDb();

app.Run();