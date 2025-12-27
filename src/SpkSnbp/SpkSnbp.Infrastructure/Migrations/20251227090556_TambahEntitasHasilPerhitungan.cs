using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TambahEntitasHasilPerhitungan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HasilPerhitungan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Jurusan = table.Column<int>(type: "integer", nullable: false),
                    TanggalPerhitungan = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MatriksPerhitungan = table.Column<double[,]>(type: "double precision[]", nullable: false),
                    MatriksTernormalisasi = table.Column<double[,]>(type: "double precision[]", nullable: false),
                    MaxKriteria = table.Column<double[]>(type: "double precision[]", nullable: false),
                    MinKriteria = table.Column<double[]>(type: "double precision[]", nullable: false),
                    MatriksTernormalisasiTerbobot = table.Column<double[,]>(type: "double precision[]", nullable: false),
                    SolusiIdealPositif = table.Column<double[]>(type: "double precision[]", nullable: false),
                    SolusiIdealNegatif = table.Column<double[]>(type: "double precision[]", nullable: false),
                    JarakSolusiIdealPositif = table.Column<double[]>(type: "double precision[]", nullable: false),
                    JarakSolusiIdealNegatif = table.Column<double[]>(type: "double precision[]", nullable: false),
                    TahunAjaranId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HasilPerhitungan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HasilPerhitungan_TahunAjaran_TahunAjaranId",
                        column: x => x.TahunAjaranId,
                        principalTable: "TahunAjaran",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HasilPerhitungan_TahunAjaranId",
                table: "HasilPerhitungan",
                column: "TahunAjaranId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HasilPerhitungan");
        }
    }
}
