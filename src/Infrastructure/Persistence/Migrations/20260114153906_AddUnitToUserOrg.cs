using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitToUserOrg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "organizational_unit_id",
                table: "user_organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_organizations_organizational_unit_id",
                table: "user_organizations",
                column: "organizational_unit_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_organizations_organizational_units_id",
                table: "user_organizations",
                column: "organizational_unit_id",
                principalTable: "organizational_units",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_organizations_organizational_units_id",
                table: "user_organizations");

            migrationBuilder.DropIndex(
                name: "ix_user_organizations_organizational_unit_id",
                table: "user_organizations");

            migrationBuilder.DropColumn(
                name: "organizational_unit_id",
                table: "user_organizations");
        }
    }
}
