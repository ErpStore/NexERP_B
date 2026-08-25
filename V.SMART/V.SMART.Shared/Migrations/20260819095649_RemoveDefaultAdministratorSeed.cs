using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V.SMART.Shared.Migrations
{
    /// <summary>
    /// Task M0-06 (risk R-09) — removes the default "Administrator" HasData seed from the
    /// EF model. This migration is DELIBERATELY A NO-OP AGAINST EXISTING DATA.
    ///
    /// WHAT THE TOOLING GENERATED, AND WHY IT WAS REPLACED
    /// `dotnet ef migrations add RemoveDefaultAdministratorSeed` scaffolded, verbatim:
    ///
    ///     Up:   migrationBuilder.DeleteData(table: "Users", keyColumn: "UserId", keyValue: 1);
    ///     Down: migrationBuilder.InsertData(table: "Users", columns: new[] { ...35 columns... },
    ///                                       values: new object[] { 1, ..., "Administrator",
    ///                                       "&lt;the published PBKDF2 hash&gt;" });
    ///
    /// Both halves were replaced by hand, for two reasons:
    ///
    ///  1. THE DELETE IS NOT SAFE TO SHIP UNATTENDED. All three foreign keys that reference
    ///     User are DeleteBehavior.Cascade — UserRight
    ///     (ApplicationDbContextModelSnapshot.cs:25924-25928), UserAuthority (:25937-25941) and
    ///     UserThemePreference (:26422-26426); the global Restrict loop in
    ///     ApplicationDbContext.OnModelCreating only rewrites relationships that are NOT
    ///     already Cascade/NoAction/Restrict, and EF defaults a required FK to Cascade. So the
    ///     DELETE would not fail — it would SUCCEED and silently cascade-delete every screen
    ///     right, approval authority and theme preference belonging to UserId 1. In a tenant
    ///     whose only administrator is the seeded account, it also locks that tenant out of its
    ///     own ERP. Per-tenant removal is therefore a supervised human step, with a pre-check —
    ///     see docs/kb/security/default-admin-removal-runbook.md (KB-104).
    ///
    ///  2. THE GENERATED Down() WOULD RE-COMMIT THE PUBLISHED CREDENTIAL. Its InsertData
    ///     carries the same PBKDF2 hash string this task exists to remove from the working
    ///     tree, which would defeat the change at the moment it landed.
    ///
    /// WHY THE MIGRATION IS KEPT AT ALL, RATHER THAN DELETED
    /// It carries the updated model snapshot. Without it, ApplicationDbContextModelSnapshot.cs
    /// would still contain the User HasData row, and the next `dotnet ef migrations add` by
    /// anyone, for any unrelated reason, would silently scaffold the DeleteData above into
    /// their migration. Keeping this migration removes that landmine.
    ///
    /// WHAT THIS MEANS FOR A DATABASE BUILT BY REPLAYING MIGRATIONS
    /// The historical 20260217110637_InitialCreate migration still INSERTs the administrator
    /// row (InitialCreate.cs:7562) and must not be edited — history is never rewritten. A
    /// database created by `dotnet ef database update` from empty therefore still receives that
    /// row, and the runbook's post-create step removes it. A database created from the MODEL
    /// (EnsureCreated, or any future scaffolding) no longer receives it at all.
    /// </summary>
    public partial class RemoveDefaultAdministratorSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty — see the class remarks. The model change (removal of the
            // User HasData seed) is carried by the accompanying model snapshot; no DML is
            // executed against an existing tenant database by this migration.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty — Up() changed no data, so there is nothing to reverse, and
            // re-inserting the default credential is exactly what this task forbids.
        }
    }
}
