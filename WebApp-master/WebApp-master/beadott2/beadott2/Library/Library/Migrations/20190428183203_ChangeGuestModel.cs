using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Library.Migrations
{
    public partial class ChangeGuestModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lending_Guest_GuestID",
                table: "Lending");

            migrationBuilder.RenameColumn(
                name: "GuestID",
                table: "Lending",
                newName: "GuestId");

            migrationBuilder.RenameIndex(
                name: "IX_Lending_GuestID",
                table: "Lending",
                newName: "IX_Lending_GuestId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Guest",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "Guest",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Guest",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Guest",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Guest",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "Guest",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "Guest",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Guest",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "Guest",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Guest",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "Guest",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Guest",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Guest",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Guest",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lending_Guest_GuestId",
                table: "Lending",
                column: "GuestId",
                principalTable: "Guest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lending_Guest_GuestId",
                table: "Lending");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Guest");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Guest");

            migrationBuilder.RenameColumn(
                name: "GuestId",
                table: "Lending",
                newName: "GuestID");

            migrationBuilder.RenameIndex(
                name: "IX_Lending_GuestId",
                table: "Lending",
                newName: "IX_Lending_GuestID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Guest",
                newName: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Lending_Guest_GuestID",
                table: "Lending",
                column: "GuestID",
                principalTable: "Guest",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
