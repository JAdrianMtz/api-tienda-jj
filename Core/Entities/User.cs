using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities;

public class User : BaseEntity
{
    public required string Name { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public ICollection<Role> Roles { get; set; } = new HashSet<Role>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
    public ICollection<UserRoles> UserRoles { get; set; } = [];
}
