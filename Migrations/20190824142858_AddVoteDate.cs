using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace voterbot.Migrations
{
    public partial class AddVoteDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VoteDate",
                table: "Votes",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoteDate",
                table: "Votes");
        }
    }
}
