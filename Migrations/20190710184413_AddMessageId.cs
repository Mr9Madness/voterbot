using Microsoft.EntityFrameworkCore.Migrations;

namespace voterbot.Migrations
{
    public partial class AddMessageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "MessageId",
                table: "Votes",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "Votes");
        }
    }
}
