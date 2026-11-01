using Api.Extensions;
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

    public async Task<IEnumerable<Product>> GetMostExpensiveProducts(int quantity, bool noTracking = true)
    {
        var queryable = noTracking
            ? _context.Products.AsNoTracking()
            : _context.Products;

        return await queryable
            .OrderByDescending(p => p.Price)
            .Take(quantity)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Product>> GetAllAsync(bool noTracking = true)
    {
        var queryable = noTracking
            ? _context.Products.AsNoTracking()
            : _context.Products;

        return await queryable
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .ToListAsync();
    }

    public override async Task<(int recordsCount, IEnumerable<Product> records)>
    GetAllAsync(int pageNumber, int pageSize, bool noTracking = true)
    {
        var queryable = noTracking
            ? _context.Products.AsNoTracking()
            : _context.Products;

        var recordsCount = await queryable.CountAsync();

        var records = await queryable
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .OrderBy(p => p.Id)
            .Paginate(pageNumber, pageSize)
            .ToListAsync();

        return (recordsCount, records);
    }

    public override async Task<Product?> GetByIdAsync(int id, bool noTracking = true)
    {
        var queryable = noTracking
            ? _context.Products.AsNoTracking()
            : _context.Products;

        return await queryable
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
