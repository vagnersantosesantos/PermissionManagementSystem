using PermissionManagement.Domain.Entities;

namespace PermissionManagement.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Permission> Permissions { get; }
    IRepository<PermissionType> PermissionTypes { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
