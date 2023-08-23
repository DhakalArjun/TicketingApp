using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketingApp.Migrations
{
    /// <inheritdoc />
    public partial class TwoCommentPropertiesAddedToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClosingComment",
                table: "Ticket",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionComment",
                table: "Ticket",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosingComment",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "ResolutionComment",
                table: "Ticket");
        }
    }
}
