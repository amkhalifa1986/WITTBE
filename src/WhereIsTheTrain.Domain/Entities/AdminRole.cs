using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class AdminRole : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<AdminRolePrivilege> Privileges { get; set; } = new List<AdminRolePrivilege>();
    public ICollection<AdminUser> AdminUsers { get; set; } = new List<AdminUser>();
}
