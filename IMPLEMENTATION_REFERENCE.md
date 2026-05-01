# EcoShopApi - Implementation Reference Guide

## Quick Fix Guide for Remaining Issues

### 1. FIX AuthService.cs - Async/Await Issues

**File**: `EcoShopApi.Application\Services\Implementation\AuthService.cs`

Replace the `CreateUserAsync` method:

```csharp
// BEFORE (BROKEN)
public Task CreateUserAsync(AppUser user, string password)
{
    var result = _userManager.CreateAsync(user, password);
    if (!result.Result.Succeeded)  // ❌ BLOCKING!
    {
        throw new Exception($"User creation failed: {string.Join(", ", result.Result.Errors.Select(e => e.Description))}");
    }
    return result;  // ❌ Returns Task, not IdentityResult
}

// AFTER (FIXED)
public async Task<IdentityResult> CreateUserAsync(AppUser user, string password)
{
    var result = await _userManager.CreateAsync(user, password);  // ✅ Proper await
    if (!result.Succeeded)
    {
        throw new DomainException(
            $"User creation failed: {string.Join(", ", result.Result.Errors.Select(e => e.Description))}",
            "USER_CREATION_FAILED"
        );
    }
    return result;
}
```

Add this method to AuthService:

```csharp
public async Task<UserDto?> RefreshAccessTokenAsync(string refreshToken)
{
    if (string.IsNullOrEmpty(refreshToken))
        throw new ArgumentException("Refresh token cannot be null or empty.", nameof(refreshToken));

    // TODO: Implement the following:
    // 1. Validate refresh token format
    // 2. Query database for stored refresh token
    // 3. Check if token is still valid (not expired, not revoked)
    // 4. Get associated AppUser
    // 5. Generate new access token
    // 6. Optionally rotate refresh token
    // 7. Return UserDto with new token

    // Temporary implementation - replace with actual logic
    throw new NotImplementedException("Refresh token implementation pending database token storage setup");
}
```

### 2. UPDATE IAuthService Interface

**File**: `EcoShopApi.Application\Services\Interface\IAuthService.cs`

Update method signature:

```csharp
public interface IAuthService
{
    Task<AppUser> GetUserByNameAsync(string userName);
    Task<AppUser> GetUserByEmailAsync(string email);
    Task<bool> UserExistsAsync(string userName);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> CheckPasswordAsync(AppUser user, string password);

    // CHANGED: Now returns Task<IdentityResult>
    Task<IdentityResult> CreateUserAsync(AppUser user, string password);

    Task UpdateUserAsync(AppUser user);
    Task<string> GenerateJwtTokenAsync(AppUser user);
    Task<string> GenerateRefreshTokenAsync();
    Task<bool> LogoutAsync(string userId, string RefreshToken);

    // NEW: Add this method
    Task<UserDto?> RefreshAccessTokenAsync(string refreshToken);
}
```

### 3. UPDATE AuthController - Fix Casting Issue

**File**: `EcoShopApi\Controllers\AuthController.cs`

The issue on line 76 will be fixed automatically once AuthService.CreateUserAsync returns Task<IdentityResult>.

For the RefreshToken endpoint, update it:

```csharp
[HttpPost("refresh-token")]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
{
    try
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(request?.Token))
            return BadRequest(new { error = "Token is required" });

        var userDto = await _authService.RefreshAccessTokenAsync(request.Token);

        if (userDto == null) 
            return Unauthorized(new { error = "Invalid or expired refresh token" });

        return Ok(userDto);
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Problem(ex.Message);
    }
}
```

### 4. Create RefreshTokenRequest DTO

**File**: `EcoShopApi.Application\DTO\AuthDTO\RefreshTokenRequest.cs` (NEW)

```csharp
namespace EcoShopApi.Application.DTO.AuthDTO;

public class RefreshTokenRequest
{
    public string Token { get; set; } = null!;
}
```

### 5. Verify All Async/Await Patterns

Run this find-and-replace to catch remaining `.Result` calls:

```bash
# Search for all .Result calls (should find ZERO matches in production code)
grep -r "\.Result" EcoShopApi/ --include="*.cs" --exclude-dir=obj --exclude-dir=bin

# Expected: Only in test mocks or sync-over-async bridge code
```

### 6. Add Missing Using Statements

Ensure all files have correct namespaces:

**ProductService.cs** should have:
```csharp
using EcoShopApi.Application.Interfaces;
using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using EcoShopApi.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
```

**AuthService.cs** should have:
```csharp
using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Application.DTO.AuthDTO;
using EcoShopApi.Domain.Entities;
using EcoShopApi.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
```

### 7. Testing Checklist

After fixes, verify:

```csharp
// ✅ ProductService
- GetProductByIdAsync returns null (not throw) for missing products
- CreateProductAsync is fully async (no .Result)
- DeleteProductAsync is async Task (not async void)
- No blocking calls to SaveProductImageAsync

// ✅ AuthService  
- CreateUserAsync returns Task<IdentityResult>
- No .Result calls anywhere
- RefreshAccessTokenAsync implemented

// ✅ Controllers
- ProductsController Delete is async Task<IActionResult>
- AuthController Register/RefreshToken work with updated AuthService
- All endpoints properly await async calls

// ✅ Domain
- Product entity has no IFormFile
- DomainException classes exist
- Error enums defined
```

---

## Build & Test Commands

```bash
# Clean build (clears all caches)
dotnet clean && dotnet build

# Run unit tests
dotnet test EcoShopApi.Tests.xUnit

# Check for async anti-patterns
dotnet roslynator analyze --severity warning

# Generate code coverage report
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

---

## Configuration Checklist

Ensure `appsettings.json` has:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=EcoShopDb;Trusted_Connection=true;"
  },
  "Jwt": {
    "Key": "your-super-secret-256-bit-key-here-minimum-32-chars",
    "Issuer": "YourAppIssuer",
    "Audience": "YourAppAudience",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

---

## Deployment Validation

After deployment to production:

```csharp
// Add this endpoint for health checking
[HttpGet("health")]
[AllowAnonymous]
public IActionResult Health()
{
    return Ok(new {
        status = "healthy",
        version = "1.0",
        timestamp = DateTime.UtcNow,
        frameworkVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
    });
}

// Monitor these metrics:
// 1. HTTP request duration (target: p99 < 500ms)
// 2. Error rate (target: < 1%)
// 3. Database connection pool usage
// 4. Memory usage (watch for leaks)
```

---

## Common Issues & Solutions

### Issue: "Cannot assign void to an implicitly-typed variable"
**Cause**: Method signature returns void instead of Task
**Solution**: Change `public async void` to `public async Task`

### Issue: "... does not contain a definition for ..."
**Cause**: Method doesn't exist on interface
**Solution**: Add method to interface, implement in class

### Issue: Deadlock on `.Result`
**Cause**: Blocking async call
**Solution**: Use `await` instead of `.Result`

### Issue: Thread pool starvation
**Cause**: Synchronous-over-async bridge
**Solution**: Convert all methods to async

---

**Next Step**: Run `dotnet build` and fix any remaining errors, then execute full test suite.
