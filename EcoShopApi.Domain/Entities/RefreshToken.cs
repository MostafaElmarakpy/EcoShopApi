using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Domain.Entities
{
    public class RefreshToken
    {
        public AppUser User { get; set; } = null!;
        public string Token { get; set; } = null!;

        public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool IsExpired => DateTime.UtcNow >= ExpireAt;
        public bool IsRevoked { get; set; }
    }
}
