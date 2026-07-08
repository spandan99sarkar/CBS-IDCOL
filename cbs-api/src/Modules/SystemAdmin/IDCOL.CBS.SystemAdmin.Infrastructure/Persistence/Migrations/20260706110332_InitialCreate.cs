using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDCOL.CBS.SystemAdmin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SYSAD_AUDIT_LOG_ENTRY",
                columns: table => new
                {
                    AUDIT_LOG_ENTRY_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ACTOR_USER_ID = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    ACTION_NAME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    ENTITY_TYPE = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    ENTITY_ID = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    DETAILS_JSON = table.Column<string>(type: "CLOB", nullable: true),
                    OCCURRED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSAD_AUDIT_LOG_ENTRY", x => x.AUDIT_LOG_ENTRY_ID);
                });

            migrationBuilder.CreateTable(
                name: "SYSAD_PERMISSION",
                columns: table => new
                {
                    PERMISSION_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CODE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSAD_PERMISSION", x => x.PERMISSION_ID);
                });

            migrationBuilder.CreateTable(
                name: "SYSAD_ROLE",
                columns: table => new
                {
                    ROLE_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CODE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    CREATED_BY = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    LAST_MODIFIED_BY = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    LAST_MODIFIED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSAD_ROLE", x => x.ROLE_ID);
                });

            migrationBuilder.CreateTable(
                name: "SYSAD_USER",
                columns: table => new
                {
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USERNAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    DISPLAY_NAME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    PASSWORD_HASH = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    BUSINESS_UNIT_CODE = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    CREATED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    LAST_MODIFIED_BY = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    LAST_MODIFIED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSAD_USER", x => x.USER_ID);
                });

            migrationBuilder.CreateTable(
                name: "SYSAD_ROLE_PERMISSION",
                columns: table => new
                {
                    PermissionsId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    RoleId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSAD_ROLE_PERMISSION", x => new { x.PermissionsId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_SYSAD_ROLE_PERMISSION_SYSAD_PERMISSION_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "SYSAD_PERMISSION",
                        principalColumn: "PERMISSION_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SYSAD_ROLE_PERMISSION_SYSAD_ROLE_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SYSAD_ROLE",
                        principalColumn: "ROLE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SYSAD_ROLE_ASSIGNMENT",
                columns: table => new
                {
                    ROLE_ASSIGNMENT_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    USER_ID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    FUNCTION_CODE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    IS_MAKER = table.Column<string>(type: "CHAR(1)", nullable: false),
                    IS_CHECKER = table.Column<string>(type: "CHAR(1)", nullable: false),
                    ASSIGNED_BY = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    ASSIGNED_AT_UTC = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSAD_ROLE_ASSIGNMENT", x => x.ROLE_ASSIGNMENT_ID);
                    table.CheckConstraint("CK_ROLE_ASSIGNMENT_NOT_BOTH", "NOT (IS_MAKER = 'Y' AND IS_CHECKER = 'Y')");
                    table.ForeignKey(
                        name: "FK_SYSAD_ROLE_ASSIGNMENT_SYSAD_USER_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "SYSAD_USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SYSAD_USER_ROLE",
                columns: table => new
                {
                    RolesId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserId = table.Column<Guid>(type: "RAW(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSAD_USER_ROLE", x => new { x.RolesId, x.UserId });
                    table.ForeignKey(
                        name: "FK_SYSAD_USER_ROLE_SYSAD_ROLE_RolesId",
                        column: x => x.RolesId,
                        principalTable: "SYSAD_ROLE",
                        principalColumn: "ROLE_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SYSAD_USER_ROLE_SYSAD_USER_UserId",
                        column: x => x.UserId,
                        principalTable: "SYSAD_USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SYSAD_AUDIT_LOG_ENTRY_OCCURRED_AT_UTC",
                table: "SYSAD_AUDIT_LOG_ENTRY",
                column: "OCCURRED_AT_UTC");

            migrationBuilder.CreateIndex(
                name: "IX_SYSAD_PERMISSION_CODE",
                table: "SYSAD_PERMISSION",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SYSAD_ROLE_CODE",
                table: "SYSAD_ROLE",
                column: "CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SYSAD_ROLE_ASSIGNMENT_USER_ID_FUNCTION_CODE",
                table: "SYSAD_ROLE_ASSIGNMENT",
                columns: new[] { "USER_ID", "FUNCTION_CODE" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SYSAD_ROLE_PERMISSION_RoleId",
                table: "SYSAD_ROLE_PERMISSION",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SYSAD_USER_USERNAME",
                table: "SYSAD_USER",
                column: "USERNAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SYSAD_USER_ROLE_UserId",
                table: "SYSAD_USER_ROLE",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SYSAD_AUDIT_LOG_ENTRY");

            migrationBuilder.DropTable(
                name: "SYSAD_ROLE_ASSIGNMENT");

            migrationBuilder.DropTable(
                name: "SYSAD_ROLE_PERMISSION");

            migrationBuilder.DropTable(
                name: "SYSAD_USER_ROLE");

            migrationBuilder.DropTable(
                name: "SYSAD_PERMISSION");

            migrationBuilder.DropTable(
                name: "SYSAD_ROLE");

            migrationBuilder.DropTable(
                name: "SYSAD_USER");
        }
    }
}
