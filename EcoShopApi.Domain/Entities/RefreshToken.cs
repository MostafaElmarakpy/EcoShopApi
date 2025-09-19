using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        public DateTime ExpireAt { get; set; }
        public bool IsRevoked { get; set; }

        // Token has expired if current time is greater than or equal to expiration time
        public bool IsExpired => DateTime.UtcNow >= ExpireAt;
        public bool IsActive => !IsExpired && !IsRevoked;
    }
}
