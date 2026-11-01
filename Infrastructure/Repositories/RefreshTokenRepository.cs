using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context) 
    { 
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool noTracking = true)
    {
        var queryable = noTracking
            ? _context.RefreshTokens.AsNoTracking()
            : _context.RefreshTokens;

        return await queryable
            .Include(rt => rt.User)
            .ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }
}
