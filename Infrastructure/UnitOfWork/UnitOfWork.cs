using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    private IBrandRepository _brands = null!;
    private ICategoryRepository _categories = null!;
    private IProductRepository _products = null!;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IBrandRepository Brands
    {
        get
        {
            if (_brands == null)
            {
                _brands = new BrandRepository(_context);
            }
            return _brands;
        }
    }

    public ICategoryRepository Categories
    {
        get
        {
            if (_categories == null)
            {
                _categories = new CategoryRepository(_context);
            }
            return _categories;
        }
    }

    public IProductRepository Products
    {
        get
        {
            if (_products == null)
            {
                _products = new ProductRepository(_context);
            }
            return _products;
        }
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
