using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class AdminRolePrivilegeConfiguration : IEntityTypeConfiguration<AdminRolePrivilege>
{
    public void Configure(EntityTypeBuilder<AdminRolePrivilege> builder)
    {
        builder.ToTable("AdminRolePrivileges");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Module).HasMaxLength(50).IsRequired();

        // Enforce unique combination of role and module
        builder.HasIndex(p => new { p.RoleId, p.Module }).IsUnique();
    }
}
