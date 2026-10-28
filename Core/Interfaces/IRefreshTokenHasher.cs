using Core.Entities;

namespace Core.Interfaces;

public interface IRefreshTokenHasher
{
    string ComputeHash(string token);
}
