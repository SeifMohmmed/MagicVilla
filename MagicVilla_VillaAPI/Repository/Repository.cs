using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    internal DbSet<T> _dbset;
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        //_context.VillaNumbers.Include(v => v.Villa);
        _dbset = _context.Set<T>();
    }
    public async Task CreateAsync(T model)
    {
        await _dbset.AddAsync(model);
        await SaveAsync();
    }
    public async Task UpdateAsync(T model)
    {
        _dbset.Update(model);
        await SaveAsync();
    }
    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null
         , int pageSize = 0, int pageNumber = 1)
    {
        IQueryable<T> query = _dbset;

        if (filter != null)
        {
            query = query.Where(filter);
        }
        if(pageSize>0)
        {
            if (pageSize > 100)
            {
                pageSize = 100;
            }
            //skip0.take(5)
            //page number- 2     || page size -5
            //skip(5*(1)) take(5)
            query = query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
        }

        if (includeProperties != null)
        {
            foreach (var prop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(prop);
            }
        }
        return await query.ToListAsync();
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeProperties = null)
    {
        IQueryable<T> query = _dbset;
        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        if (filter != null)
        {
            query = query.Where(filter);
        }
        if (includeProperties != null)
        {
            foreach (var prop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(prop);
            }
        }
        return await query.FirstOrDefaultAsync();
    }

    public async Task RemoveAsync(T model)
    {
        _dbset.Remove(model);
        await SaveAsync();
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
