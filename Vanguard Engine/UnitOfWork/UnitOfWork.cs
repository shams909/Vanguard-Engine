using Vanguard_Engine.Data;
using Vanguard_Engine.Repositories;

namespace Vanguard_Engine.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context, IRoleRepository roles, IUserRepository users)
    {
        _context = context;
        Roles = roles;
        Users = users;
    }

    public IRoleRepository Roles { get; }
    public IUserRepository Users { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
