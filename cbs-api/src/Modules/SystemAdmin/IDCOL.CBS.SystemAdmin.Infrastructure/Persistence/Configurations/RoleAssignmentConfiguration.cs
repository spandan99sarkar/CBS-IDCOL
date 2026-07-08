using IDCOL.CBS.SystemAdmin.Domain.Entities;
using IDCOL.CBS.SystemAdmin.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Persistence.Configurations;

public class RoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment>
{
    public void Configure(EntityTypeBuilder<RoleAssignment> builder)
    {
        // Layer 3 of 3 (database) of the structural maker-checker enforcement described in the
        // architecture plan: this survives even an application-layer bug, unlike the domain
        // (layer 1) and MediatR pipeline (layer 2) checks.
        builder.ToTable("SYSAD_ROLE_ASSIGNMENT", t => t.HasCheckConstraint(
            "CK_ROLE_ASSIGNMENT_NOT_BOTH",
            "NOT (IS_MAKER = 'Y' AND IS_CHECKER = 'Y')"));

        builder.HasKey(ra => ra.Id);
        builder.Property(ra => ra.Id).HasColumnName("ROLE_ASSIGNMENT_ID").ValueGeneratedNever();
        builder.Property(ra => ra.UserId).HasColumnName("USER_ID").IsRequired();

        builder.Property(ra => ra.FunctionCode)
            .HasColumnName("FUNCTION_CODE")
            .HasConversion(fc => fc.Value, value => FunctionCode.Of(value))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ra => ra.IsMaker)
            .HasColumnName("IS_MAKER")
            .HasConversion(v => v ? "Y" : "N", v => v == "Y")
            .HasColumnType("CHAR(1)")
            .IsRequired();

        builder.Property(ra => ra.IsChecker)
            .HasColumnName("IS_CHECKER")
            .HasConversion(v => v ? "Y" : "N", v => v == "Y")
            .HasColumnType("CHAR(1)")
            .IsRequired();

        builder.Property(ra => ra.AssignedBy).HasColumnName("ASSIGNED_BY").HasMaxLength(50).IsRequired();
        builder.Property(ra => ra.AssignedAtUtc).HasColumnName("ASSIGNED_AT_UTC");

        // A user can hold at most one role assignment per function. Combined with the CHECK
        // constraint above (a single row can't be both Maker and Checker), this makes it
        // impossible for one user to end up Maker AND Checker for the same function even via
        // two separate rows.
        builder.HasIndex(ra => new { ra.UserId, ra.FunctionCode }).IsUnique();
    }
}
