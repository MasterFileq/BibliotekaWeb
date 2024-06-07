using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotekaWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class act_21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wypozyczenie_Czytelnik_CzytelnikId1",
                table: "Wypozyczenie");

            migrationBuilder.DropIndex(
                name: "IX_Wypozyczenie_CzytelnikId1",
                table: "Wypozyczenie");

            migrationBuilder.DropColumn(
                name: "CzytelnikId1",
                table: "Wypozyczenie");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TerminZwrotu",
                table: "Wypozyczenie",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CzytelnikId",
                table: "Wypozyczenie",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Wypozyczenie_CzytelnikId",
                table: "Wypozyczenie",
                column: "CzytelnikId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wypozyczenie_AspNetUsers_CzytelnikId",
                table: "Wypozyczenie",
                column: "CzytelnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wypozyczenie_AspNetUsers_CzytelnikId",
                table: "Wypozyczenie");

            migrationBuilder.DropIndex(
                name: "IX_Wypozyczenie_CzytelnikId",
                table: "Wypozyczenie");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TerminZwrotu",
                table: "Wypozyczenie",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CzytelnikId",
                table: "Wypozyczenie",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "CzytelnikId1",
                table: "Wypozyczenie",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Wypozyczenie_CzytelnikId1",
                table: "Wypozyczenie",
                column: "CzytelnikId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Wypozyczenie_Czytelnik_CzytelnikId1",
                table: "Wypozyczenie",
                column: "CzytelnikId1",
                principalTable: "Czytelnik",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
