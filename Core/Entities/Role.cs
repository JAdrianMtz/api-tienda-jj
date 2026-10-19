using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities;

public class Role : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<User> Users { get; set; } = new HashSet<User>();
    public ICollection<UserRoles> UserRoles { get; set; } = [];
}
