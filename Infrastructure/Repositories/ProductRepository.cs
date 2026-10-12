using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) 
    { 
    }

    public async Task<IEnumerable<Product>> GetMostExpensiveProducts(int quantity)
    {
        return await _context.Products
            .OrderByDescending(p => p.Price)
            .Take(quantity)
            .ToListAsync();
    }
}
