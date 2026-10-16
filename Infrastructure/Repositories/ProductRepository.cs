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

    public async override Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .ToListAsync();
    }

    public override async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

    }

    public override void Add(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        base.Add(product);
    }
}
