using Api.Extensions;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(bool noTracking = true)
    {
        var queryable = noTracking
            ? _context.Set<T>().AsNoTracking()
            : _context.Set<T>();

        return await queryable.ToListAsync();
    }

    public virtual async Task<(int recordsCount, IEnumerable<T> records)> GetAllAsync(
        int pageNumber, int pageSize, bool noTracking = true)
    {
        var queryable = noTracking
            ? _context.Set<T>().AsNoTracking()
            : _context.Set<T>();

        var recordsCount = await queryable.CountAsync();

        var records = await queryable
            .OrderBy(t => t.Id)
            .Paginate(pageNumber, pageSize)
            .ToListAsync();

        return (recordsCount, records);

    }

    public virtual async Task<T?> GetByIdAsync(int id, bool noTracking = true)
    {
        var queryable = noTracking
            ? _context.Set<T>().AsNoTracking()
            : _context.Set<T>();

        return await queryable.FirstOrDefaultAsync(t => t.Id == id);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool noTracking = true)
    {
        var queryable = noTracking
            ? _context.Set<T>().AsNoTracking()
            : _context.Set<T>();

        return await queryable
            .Where(predicate)
            .ToListAsync();
    }

    public virtual void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    public virtual void AddRange(IEnumerable<T> entities)
    {
        _context.Set<T>().AddRange(entities);
    }

    public virtual void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        _context.Set<T>().RemoveRange(entities);
    }

    public virtual void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }
}
