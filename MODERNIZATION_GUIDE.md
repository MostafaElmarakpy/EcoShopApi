# EcoShopApi: Comprehensive Modernization & Migration Guide

**Generated**: 2025
**Target**: .NET 9 → .NET 10 Upgrade + Architecture Refactoring
**Status**: PRODUCTION-READY PLAN (Phase-based implementation)

---

## EXECUTIVE SUMMARY

**Current State**: Your EcoShopApi has a **3-layer architecture with critical async/await anti-patterns** that must be fixed before production deployment.

**Severity**: 🔴 **CRITICAL** - Current code has deadlock risks and thread pool starvation bugs.

**Recommended Timeline**: 
- **Phase 1 (Week 1)**: Critical async fixes + error abstractions
- **Phase 2 (Week 2)**: Service refactoring + interface cleanup
- **Phase 3 (Week 3)**: Integration testing + deployment
- **Phase 4 (Week 4+)**: .NET 10 upgrade

---

## SECTION 1: DETAILED ARCHITECTURAL ANALYSIS

### 1.1 Current Architecture Overview

```
EcoShopApi (Presentation)
  ├── Controllers (ProductsController, AuthController, UsersController)
  ├── Middlewares (ExceptionMiddleware)
  ├── Errors (ApiResponse, ApiValidationErrorResponse, ApiExceptionResponse)
  └── Program.cs (Bootstrap)
          ↓
EcoShopApi.Application (Business Logic)
  ├── Services (AuthService, UserService, ProductService)
  ├── Interfaces (IAuthService, IUserService, IProductService)
  ├── DTOs (UserDTO, AuthDTO, ProductDTO)
  ├── Mapping (AutoMapperProfile)
  └── Repositories (Generic + Specialized)
          ↓
EcoShopApi.Infrastructure (Data Access)
  ├── Repository (GenaricRepository, ProductRepository, UserRepository, UnitOfWork)
  ├── Data (ApplicationDbContext, DbInitializer, Entity Configurations)
  └── Migrations
          ↓
EcoShopApi.Domain (Core Business Rules)
  └── Entities (Product, Category, User, RefreshToken)
```

### 1.2 Critical Issues Found

| # | Issue | Severity | Location | Impact |
|---|-------|----------|----------|--------|
| 1 | **Async/await misuse with .Result** | 🔴 CRITICAL | ProductService.cs:26, ProductsController.cs:90 | DEADLOCKS |
| 2 | **Inconsistent async contracts** | 🔴 CRITICAL | IProductService, IUnitOfWork | Thread pool starvation |
| 3 | **Async void methods** | 🔴 CRITICAL | ProductService.DeleteProductAsync() | Unhandled exceptions |
| 4 | **IFormFile in Domain Entity** | 🟠 HIGH | Product.cs | Layer coupling |
| 5 | **Generic exceptions only** | 🟠 HIGH | All services | Poor error handling |
| 6 | **Typo: "Genaric"** | 🟡 MEDIUM | IGenaricRepository | Code quality |
| 7 | **God services** | 🟠 HIGH | ProductService, UserService | Single responsibility violation |
| 8 | **No validation layer** | 🟡 MEDIUM | All services | Business rule enforcement |
| 9 | **Mixed .NET framework versions** | 🟠 HIGH | .csproj files | Compilation conflicts |
| 10 | **Broken Program.cs** | 🔴 CRITICAL | Program.cs | Build failure |

---

## SECTION 2: DETAILED CODE FIXES (Already Implemented)

### ✅ Completed Changes

1. ✅ **Fixed target framework inconsistency** (net10.0 → net9.0 for all projects)
2. ✅ **Created Result<T> error abstraction** (EcoShopApi.Application/Common/Result.cs)
3. ✅ **Created domain exceptions** (ProductNotFoundException, CategoryNotFoundException)
4. ✅ **Fixed IProductService async contracts** (all methods now properly async)
5. ✅ **Refactored ProductService** (removed .Result calls, added logging, proper async)
6. ✅ **Fixed ProductsController** (endpoints now properly async)
7. ✅ **Removed IFormFile from Product entity** (domain now pure)
8. ✅ **Rewrote Program.cs** (working .NET 9 baseline)

---

## SECTION 3: REMAINING FIXES NEEDED

### 3.1 AuthService Issues

**Problem**: CreateUserAsync returns Task instead of Task<IdentityResult>

**Current Code**:
```csharp
public Task CreateUserAsync(AppUser user, string password)
{
    var result = _userManager.CreateAsync(user, password);
    if (!result.Result.Succeeded)  // ⚠️ Using .Result - ANTI-PATTERN
    {
        throw new Exception($"User creation failed: ...");
    }
    return result;  // ⚠️ Returns Task, not Task<IdentityResult>
}
```

**Fix**:
```csharp
public async Task<IdentityResult> CreateUserAsync(AppUser user, string password)
{
    var result = await _userManager.CreateAsync(user, password);  // ✅ Proper await
    if (!result.Succeeded)
    {
        throw new DomainException(
            $"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}",
            "USER_CREATION_FAILED"
        );
    }
    return result;
}
```

**Update IAuthService Interface**:
```csharp
public interface IAuthService
{
    // ... other methods ...
    Task<IdentityResult> CreateUserAsync(AppUser user, string password);  // Changed!
}
```

### 3.2 RefreshToken Implementation

**Problem**: RefreshAccessTokenAsync method doesn't exist

**Required Addition**:
```csharp
public class UserDto
{
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public string? RefreshToken { get; set; }
}

// In IAuthService
Task<UserDto?> RefreshAccessTokenAsync(string refreshToken);

// In AuthService
public async Task<UserDto?> RefreshAccessTokenAsync(string refreshToken)
{
    // TODO: Implement token validation and refresh logic
    // 1. Validate refresh token
    // 2. Get associated user
    // 3. Generate new access token
    // 4. Return UserDto with new token
    throw new NotImplementedException("Refresh token implementation pending");
}
```

### 3.3 AuthController Issues

**Problem**: AuthController expects IdentityResult but AuthService throws exceptions

**Fix** (Already shows the issue):
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
{
    // ... validation ...

    var user = new AppUser
    {
        UserName = registerDto.UserName,
        Email = registerDto.Email,
        EmailConfirmed = true,
        DisplayName = registerDto.UserName ?? registerDto.UserName,
    };

    try
    {
        var result = await _authService.CreateUserAsync(user, registerDto.Password);  // ✅ Now returns IdentityResult

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new UserDto
        {
            DisplayName = user.UserName,
            Email = user.Email,
            Token = await _authService.GenerateJwtTokenAsync(user)
        });
    }
    catch (DomainException ex)
    {
        return BadRequest(new { Error = ex.Message, Code = ex.Code });
    }
}
```

---

## SECTION 4: STEP-BY-STEP MIGRATION PLAN (.NET 9 → .NET 10)

### Phase A: Pre-Upgrade Stabilization (Current Week)

1. **Fix all async/await issues** ✅ (ProductService, ProductsController)
2. **Fix auth issues** (AuthService, AuthController)
3. **Create comprehensive unit tests** (cover all services)
4. **Create integration tests** (cover all API endpoints)
5. **Perform load testing** (detect any remaining thread pool issues)
6. **Deploy to staging** (verify all functionality)

### Phase B: .NET 10 Preparation (Week 2)

#### Step 1: Update Target Frameworks
```xml
<!-- Update all projects to .NET 10 -->
<TargetFramework>net10.0</TargetFramework>
```

#### Step 2: Update NuGet Packages
```bash
dotnet package upgrade  # Auto-upgrades all packages

# Or manually:
dotnet package update Microsoft.EntityFrameworkCore --version 10.0.0
dotnet package update Microsoft.AspNetCore.Identity --version 10.0.0
dotnet package update Swashbuckle.AspNetCore --version 11.0.0  # For .NET 10
dotnet package update Microsoft.IdentityModel.Tokens --version 9.0.0
```

#### Step 3: Update Program.cs for .NET 10 Features

**Enhanced Program.cs with .NET 10 features**:
```csharp
using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Application.Services.Implementation;
using EcoShopApi.Application.Mapping;
using EcoShopApi.Infrastructure.Data;
using EcoShopApi.Infrastructure.Repository;
using EcoShopApi.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using EcoShopApi.Domain.Entities;
using EcoShopApi.Middlewares;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

// .NET 10: Enhanced DbContext with query tracking optimization
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// Identity configuration
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Repository and Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = System.Text.Encoding.UTF8.GetBytes(
    jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// .NET 10: Enhanced rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Host.Host,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EcoShop API",
        Version = "v1.0",
        Description = "Eco-friendly products e-commerce API - ASP.NET 10"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference(ReferenceType.SecurityScheme, "Bearer") }, [] }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// .NET 10: Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(name: "database");

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EcoShop API v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");

// .NET 10: Rate limiter middleware
app.UseRateLimiter();

// Health check endpoint
app.MapHealthChecks("/health");

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    dbInitializer.Initialize();
}

app.Run();

public partial class Program { }
```

---

## SECTION 5: RISKS & MITIGATION STRATEGIES

### 5.1 Database-Related Risks

| Risk | Mitigation |
|------|-----------|
| Migration failures on large tables | Run migrations in development first, test with production data backup |
| Connection string format changes | Pre-configure all connection strings in configuration |
| EF Core query behavior changes | Run comprehensive integration tests for all queries |

**Mitigation Action Plan**:
```csharp
// 1. Create database backup before migration
// 2. Test migrations on staging with production-like data
// 3. Use Entity Framework migration scripts review

// Program.cs - Add migration verification
app.Services.GetRequiredService<ApplicationDbContext>().Database.Migrate();
```

### 5.2 Authentication/Authorization Risks

| Risk | Mitigation |
|------|-----------|
| JWT token validation changes | Update token validation parameters |
| Identity API changes | Test all identity operations |
| Refresh token handling | Implement token revocation list (TRL) |

**Mitigation Code**:
```csharp
// Comprehensive JWT testing
[Test]
public async Task ValidateTokenAfterUpgrade()
{
    // Ensure old tokens still validate
    var oldToken = GenerateTestToken(DateTime.UtcNow.AddDays(-1));
    var isValid = ValidateToken(oldToken);
    Assert.IsTrue(isValid, "Old tokens should still validate");
}
```

### 5.3 Performance Risks

| Risk | Mitigation |
|------|-----------|
| Query performance degradation | Profile queries before/after upgrade |
| Thread pool exhaustion | Use async/await consistently (already fixed) |
| Memory leaks from disposal changes | Test memory usage under load |

**Monitoring Setup**:
```csharp
// Add application insights for monitoring
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.EnableAdaptiveSampling = true;
    options.RequestCollectionOptions.TrackExceptions = true;
});

// Monitor specific metrics
app.MapHealthChecks("/health/detailed", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = WriteDetailedResponse
});
```

### 5.4 API Compatibility Risks

| Risk | Mitigation |
|------|-----------|
| Serialization format changes | Implement response format tests |
| HTTP header handling changes | Review middleware pipeline |
| Status code behavior changes | Test error responses |

**Serialization Test**:
```csharp
[Test]
public async Task ApiResponseFormatConsistency()
{
    var response = await GetProductAsync(1);
    var json = JsonSerializer.Serialize(response);

    // Ensure format unchanged
    Assert.Contains("\"data\":", json);
    Assert.Contains("\"isSuccess\":", json);
}
```

---

## SECTION 6: COMPREHENSIVE TESTING STRATEGY

### 6.1 Unit Testing Plan

**Setup xUnit Test Project Structure**:
```
EcoShopApi.Tests.xUnit/
├── Services/
│   ├── ProductServiceTests.cs
│   ├── AuthServiceTests.cs
│   ├── UserServiceTests.cs
│   └── TokenServiceTests.cs
├── Repositories/
│   ├── ProductRepositoryTests.cs
│   └── GenericRepositoryTests.cs
├── Controllers/
│   ├── ProductsControllerTests.cs
│   ├── AuthControllerTests.cs
│   └── UsersControllerTests.cs
├── Fixtures/
│   └── TestData.cs
└── Mocks/
    ├── MockUnitOfWork.cs
    └── MockServices.cs
```

**Example Unit Test**:
```csharp
using Xunit;
using Moq;
using EcoShopApi.Application.Services.Implementation;
using EcoShopApi.Application.Interfaces;
using EcoShopApi.Domain.Entities;
using EcoShopApi.Domain.Exceptions;
using Microsoft.Extensions.Logging;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var expectedProduct = new Product 
        { 
            Id = productId, 
            Name = "Test Product", 
            Price = 99.99m 
        };

        _mockUnitOfWork.Setup(u => u.Product.Get(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), 
            null))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProduct.Id, result.Id);
        Assert.Equal("Test Product", result.Name);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.Product.Get(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), 
            null))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetProductByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteProductAsync_WithNonExistentProduct_ThrowsException()
    {
        // Arrange
        _mockUnitOfWork.Setup(u => u.Product.Get(
            It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), 
            null))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(
            () => _productService.DeleteProductAsync(999));
    }
}
```

### 6.2 Integration Testing Plan

**Integration Test Example**:
```csharp
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using EcoShopApi;

public class ProductsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Use in-memory database for testing
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllProducts_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var productDto = new ProductCreateDto
        {
            Name = "Test Product",
            ProductCode = "TP001",
            Price = 99.99m,
            CategoryId = 1
        };

        var json = JsonSerializer.Serialize(productDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

### 6.3 Load Testing Strategy

**Using k6 or Apache JMeter**:
```javascript
// load-test.js (k6 script)
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    vus: 100,           // Virtual users
    duration: '30s',    // Test duration
    thresholds: {
        http_req_duration: ['p(99)<500'],  // 99% of requests < 500ms
        http_req_failed: ['rate<0.1']      // Failure rate < 10%
    }
};

export default function() {
    // Test API endpoints
    let response = http.get('http://localhost:5000/api/products');
    check(response, {
        'status is 200': (r) => r.status === 200,
        'response time < 500ms': (r) => r.timings.duration < 500
    });
    sleep(1);
}
```

---

## SECTION 7: DEPLOYMENT STRATEGY

### 7.1 Blue-Green Deployment

```bash
# 1. Deploy new version (.NET 10) to Green environment
dotnet publish -c Release -o ./publish

# 2. Run comprehensive tests on Green
# Run smoke tests, integration tests, load tests

# 3. Switch traffic from Blue to Green
# Use load balancer or DNS swap

# 4. Keep Blue running as fallback for 24 hours
```

### 7.2 Rollback Plan

```csharp
// In case of critical issues, rollback to previous version
// 1. Identify the issue using application insights
// 2. Switch traffic back to Blue (previous version)
// 3. Investigate root cause
// 4. Fix and retest before re-deploying
```

---

## SECTION 8: ADDITIONAL IMPROVEMENTS (POST-MIGRATION)

### 8.1 Add Logging Across All Layers

```csharp
// Install Serilog for structured logging
dotnet add package Serilog
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Enrichers.Context

// Configure in Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
```

### 8.2 Add Caching

```csharp
// Add Redis caching
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis

// Register in Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Use in ProductService
public class ProductService
{
    private readonly IDistributedCache _cache;

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var cacheKey = $"product_{id}";
        var cachedProduct = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedProduct))
            return JsonSerializer.Deserialize<Product>(cachedProduct);

        var product = await _unitOfWork.Product.Get(p => p.Id == id);
        if (product != null)
        {
            await _cache.SetStringAsync(cacheKey, 
                JsonSerializer.Serialize(product), 
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
        }

        return product;
    }
}
```

### 8.3 Add API Versioning

```csharp
// Install versioning package
dotnet add package Microsoft.AspNetCore.Mvc.Versioning

// Configure in Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});

// Use in controllers
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    // ...
}
```

### 8.4 Add CQRS Pattern (Optional)

```csharp
// Install MediatR
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection

// Create query/command handlers
public class GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public int ProductId { get; set; }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Product.Get(p => p.Id == request.ProductId);
        if (product == null)
            return Result<ProductDto>.Failure("Product not found", ErrorCode.ProductNotFound);

        return Result<ProductDto>.Success(new ProductDto { /* map from product */ });
    }
}
```

---

## SECTION 9: CHECKLIST FOR GO-LIVE

- [ ] All unit tests passing (>80% coverage)
- [ ] All integration tests passing
- [ ] Load testing completed (target: 1000 req/sec)
- [ ] Security review completed (JWT, authentication)
- [ ] Database migrations tested on production-like data
- [ ] Rollback procedure documented and tested
- [ ] Logging and monitoring configured
- [ ] CORS policy reviewed and hardened
- [ ] Rate limiting configured appropriately
- [ ] Error handling tested for all edge cases
- [ ] Documentation updated for new .NET 10 features
- [ ] Team trained on new deployment process
- [ ] Stakeholders informed of deployment window

---

## SECTION 10: QUICK REFERENCE - BREAKING CHANGES .NET 9→10

| Feature | .NET 9 | .NET 10 | Action |
|---------|--------|---------|--------|
| Default JSON naming | CamelCase | PascalCase | Add `JsonNamingPolicy` if needed |
| EF Core tracking | Default | Optimized | Review query performance |
| Rate limiting | Manual | Built-in | Use new RateLimiter |
| OpenAPI | Swagger | Native | Migrate to `AddOpenApi` |
| JWT defaults | Manual | Enhanced | Review token parameters |

---

## CONCLUSION

Your codebase has solid foundations but requires **Phase 1 critical fixes** before proceeding with .NET 10 upgrade. Follow the provided step-by-step plan, test thoroughly, and deploy with confidence.

**Next Steps**:
1. Implement remaining async/await fixes (AuthService)
2. Complete unit test suite
3. Schedule migration window
4. Execute Phase-based rollout
5. Monitor production metrics post-deployment

**Questions?** Refer back to specific section for detailed guidance.

---

**Document Version**: 1.0
**Last Updated**: 2025
**Target Audience**: Development Team, DevOps, QA, Management
