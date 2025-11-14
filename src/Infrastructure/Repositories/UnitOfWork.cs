using Microsoft.EntityFrameworkCore.Storage;
using PermissionManagement.Domain.Entities;
using PermissionManagement.Domain.Interfaces;
using PermissionManagement.Infrastructure.Data;

namespace PermissionManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly PermissionDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IRepository<Permission>? _permissions;
    private IRepository<PermissionType>? _permissionTypes;

    public UnitOfWork(PermissionDbContext context)
    {
        _context = context;
    }

    public IRepository<Permission> Permissions => 
        _permissions ??= new Repository<Permission>(_context);

    public IRepository<PermissionType> PermissionTypes => 
        _permissionTypes ??= new Repository<PermissionType>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
