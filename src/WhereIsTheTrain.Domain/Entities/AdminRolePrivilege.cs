using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class AdminRolePrivilege : BaseEntity
{
    public Guid RoleId { get; set; }
    public AdminRole Role { get; set; } = null!;
    public string Module { get; set; } = string.Empty; // e.g. "Trains", "Trips", etc.
    public bool CanView { get; set; } = false;
    public bool CanAdd { get; set; } = false;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
}
