using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, bool noTracking = true)
    {
        var queryable = noTracking
            ? _context.Users.AsNoTracking()
            : _context.Users;

        return await queryable
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == username);
    }
}
