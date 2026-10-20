using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces;

public interface IUnitOfWork
{
    IBrandRepository Brands { get; }
    ICategoryRepository Categories { get; }
    IProductRepository Products { get; }
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    Task<int> SaveAsync();
}
