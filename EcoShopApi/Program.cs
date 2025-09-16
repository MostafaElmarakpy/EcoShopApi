using EcoShopApi.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT authentication
var jwtCfg = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtCfg["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtCfg["Issuer"],
        ValidAudience = jwtCfg["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

// Services: TokenService, Repositories, etc.
//builder.Services.AddScoped<ITokenService, TokenService>();
//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}


app.UseAuthorization();


app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
