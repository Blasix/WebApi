using LU2.Repositories;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

builder.Services.AddAuthorization();

string? connStr = builder.Configuration["SqlConnectionString"];

builder.Services.AddIdentityApiEndpoints<IdentityUser>(options =>

    {

        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 10;

    }).AddRoles<IdentityRole>()

    .AddDapperStores(options =>

    {

        options.ConnectionString = connStr;

    });

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
bool sqlConnectionStringFound = !string.IsNullOrWhiteSpace(connStr); // Check if the connection string is found

if (sqlConnectionStringFound)
{
    // Register repositories
    builder.Services.AddTransient<IEnvironment2DRepository, Environment2DRepository>(provider => 
        new Environment2DRepository(connStr));
    builder.Services.AddTransient<IObject2DRepository, Object2DRepository>(provider => 
        new Object2DRepository(connStr));
}

var app = builder.Build();

// Web API routes

app.MapGet("/", () => $"The API is up. \n Connection string found: {(sqlConnectionStringFound ? "Ok" : "Nope")}");

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())

{

    app.UseSwagger();

    app.UseSwaggerUI();

}

app.UseAuthorization();

app.MapControllers();

app.MapGroup("/account").MapIdentityApi<IdentityUser>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();