using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Vanguard_Engine.Data;

namespace Vanguard_Engine.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public Task<T?> GetByIdAsync(int id) => DbSet.FindAsync(id).AsTask();

    public Task<List<T>> GetPagedAsync(int pageNumber, int pageSize) =>
        DbSet.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

    public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        DbSet.Where(predicate).ToListAsync();

    public Task AddAsync(T entity) => DbSet.AddAsync(entity).AsTask();

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);
}
