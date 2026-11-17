using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataEntity.Migrations
{
    /// <inheritdoc />
    public partial class PocatecniSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mistnosti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nazev = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kapacita = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DatumVytvoreni = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mistnosti", x => x.Id);
                    table.CheckConstraint("CK_Mistnosti_Kapacita", "[Kapacita] > 0");
                });

            migrationBuilder.CreateTable(
                name: "Zamestnanci",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Jmeno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prijmeni = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DatumVytvoreni = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zamestnanci", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rezervace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MistnostId = table.Column<int>(type: "int", nullable: false),
                    ZamestnanecId = table.Column<int>(type: "int", nullable: false),
                    CasOd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CasDo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ucel = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    DatumVytvoreni = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezervace", x => x.Id);
                    table.CheckConstraint("CK_Rezervace_Interval", "[CasDo] > [CasOd] AND [CasDo] <= DATEADD(hour, 8, [CasOd])");
                    table.CheckConstraint("CK_Rezervace_Ucel", "LEN(LTRIM(RTRIM([Ucel]))) > 0");
                    table.ForeignKey(
                        name: "FK_Rezervace_Mistnosti_MistnostId",
                        column: x => x.MistnostId,
                        principalTable: "Mistnosti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rezervace_Zamestnanci_ZamestnanecId",
                        column: x => x.ZamestnanecId,
                        principalTable: "Zamestnanci",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervace_DatumVytvoreni",
                table: "Rezervace",
                column: "DatumVytvoreni");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervace_MistnostId_CasOd_CasDo",
                table: "Rezervace",
                columns: new[] { "MistnostId", "CasOd", "CasDo" });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervace_ZamestnanecId_CasOd_CasDo",
                table: "Rezervace",
                columns: new[] { "ZamestnanecId", "CasOd", "CasDo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rezervace");

            migrationBuilder.DropTable(
                name: "Mistnosti");

            migrationBuilder.DropTable(
                name: "Zamestnanci");
        }
    }
}
