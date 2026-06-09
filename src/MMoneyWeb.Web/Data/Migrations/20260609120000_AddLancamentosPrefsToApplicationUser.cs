using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MMoneyWeb.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLancamentosPrefsToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LancamentosIdCompetencia",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LancamentosIdContaCorrente",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LancamentosIdCompetencia",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LancamentosIdContaCorrente",
                table: "AspNetUsers");
        }
    }
}
