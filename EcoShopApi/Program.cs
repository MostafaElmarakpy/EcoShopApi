using EcoShopApi.Application.Common.Interfaces;
using EcoShopApi.Application.Services.Implementation;
using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using EcoShopApi.Infrastructure.Data;
using EcoShopApi.Infrastructure.Repository;
using EcoShopApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1) Register services (DbContext, Identity, Repos, Services, etc.)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (ensure AppUser type and ApplicationDbContext are correct)
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT auth
var jwtCfg = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtCfg["Key"] ?? throw new InvalidOperationException("Jwt:Key missing"));


builder.Services.AddCors(options =>
  options.AddPolicy("AllowAngularDev", policy =>
    policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials()
  )
);


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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCfg["Jwt:Key"])),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddControllers();

// Register application services / repositories
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

//Apply migrations & seed database using app.Services (after Build)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var dbInitializer = services.GetRequiredService<IDbInitializer>();

        // apply pending migrations
        context.Database.Migrate();

        // call seeding initializer (make it synchronous or use async/Wait if needed)
        dbInitializer.Initialize();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
        // Optionally: if you want to inspect loader exceptions (ReflectionTypeLoadException)
        if (ex is ReflectionTypeLoadException rtle)
        {
            foreach (var le in rtle.LoaderExceptions)
            {
                logger.LogError("LoaderException: {Message}", le.Message);
            }
        }
    }
}

// Middleware and HTTP pipeline
app.UseMiddleware<ExceptionMiddleware>();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}
    app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
