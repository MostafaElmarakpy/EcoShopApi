using EcoShopApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Services.Interface
{
    public interface ITokenService
    {
        string CreateAccessToken(AppUser user);
        RefreshToken CreateRefreshToken();
        bool ValidateRefreshToken(RefreshToken token, string refreshToken);




    }
}
