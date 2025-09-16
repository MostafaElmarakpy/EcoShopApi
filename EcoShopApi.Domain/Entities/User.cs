using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
