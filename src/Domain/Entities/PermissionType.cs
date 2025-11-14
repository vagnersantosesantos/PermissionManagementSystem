namespace PermissionManagement.Domain.Entities;

public class PermissionType
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // Navigation property
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
