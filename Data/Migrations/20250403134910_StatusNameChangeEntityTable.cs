using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class StatusNameChangeEntityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_StatusTypes_StatusId",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StatusTypes",
                table: "StatusTypes");

            migrationBuilder.RenameTable(
                name: "StatusTypes",
                newName: "Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Status",
                table: "Status",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Status_StatusId",
                table: "Projects",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Status_StatusId",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Status",
                table: "Status");

            migrationBuilder.RenameTable(
                name: "Status",
                newName: "StatusTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StatusTypes",
                table: "StatusTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_StatusTypes_StatusId",
                table: "Projects",
                column: "StatusId",
                principalTable: "StatusTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
