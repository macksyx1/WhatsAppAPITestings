using System.Security.Claims;
using WhatsAppTestLog.Models;

namespace WhatsAppTestLog.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        ClaimsPrincipal ValidateToken(string token);
    }
}
