using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WalletAPI.Services.Data.Migrations
{
    public partial class UpdatedChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "UserCurrencies",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserCurrencies",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserCurrencies",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCurrencies",
                table: "UserCurrencies",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCurrencies",
                table: "UserCurrencies");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserCurrencies");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserCurrencies");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserCurrencies");
        }
    }
}
