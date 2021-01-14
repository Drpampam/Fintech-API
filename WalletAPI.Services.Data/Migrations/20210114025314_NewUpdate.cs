using Microsoft.EntityFrameworkCore.Migrations;

namespace WalletAPI.Services.Data.Migrations
{
    public partial class NewUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_UsersId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UsersId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Transactions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "UsersId",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UsersId",
                table: "Transactions",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_UsersId",
                table: "Transactions",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
