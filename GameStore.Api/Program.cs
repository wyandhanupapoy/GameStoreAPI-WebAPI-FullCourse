using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();

var connString = "DataSource=GameStore.db";
builder.Services.AddSqlite<GameStoreContext>(connString);

var app = builder.Build();

app.MapGamesEndpoints();

app.MigrateDb();

app.Run();