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
    public required string Password { get; set; }
    public ICollection<Role> Roles { get; set; } = new HashSet<Role>();
    public ICollection<UserRoles> UserRoles { get; set; } = [];
}
