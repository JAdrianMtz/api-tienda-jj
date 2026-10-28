using Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security;

public class RefreshTokenHasher : IRefreshTokenHasher
{
    public string ComputeHash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(bytes);

        return Convert.ToBase64String(hashBytes);
    }
}
