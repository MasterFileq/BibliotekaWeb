using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotekaWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class act_9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wypozyczenie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KsiazkaId = table.Column<int>(type: "int", nullable: false),
                    CzytelnikId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DataWypozyczenia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TerminZwrotu = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wypozyczenie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wypozyczenie_AspNetUsers_CzytelnikId",
                        column: x => x.CzytelnikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wypozyczenie_Ksiazka_KsiazkaId",
                        column: x => x.KsiazkaId,
                        principalTable: "Ksiazka",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wypozyczenie_CzytelnikId",
                table: "Wypozyczenie",
                column: "CzytelnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Wypozyczenie_KsiazkaId",
                table: "Wypozyczenie",
                column: "KsiazkaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wypozyczenie");
        }
    }
}
