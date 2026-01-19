using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TambahInformasiSekolah : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InformasiSekolah",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NPSN = table.Column<string>(type: "text", nullable: false),
                    NamaSekolah = table.Column<string>(type: "text", nullable: false),
                    BentukPendidikan = table.Column<string>(type: "text", nullable: false),
                    Akreditasi = table.Column<string>(type: "text", nullable: false),
                    Nilai = table.Column<double>(type: "double precision", nullable: false),
                    NoSKAkreditasi = table.Column<string>(type: "text", nullable: false),
                    TanggalSKAkreditasi = table.Column<DateOnly>(type: "date", nullable: false),
                    TMTMulaiSKAkreditasi = table.Column<DateOnly>(type: "date", nullable: false),
                    TMTSelesaiSKAkreditasi = table.Column<DateOnly>(type: "date", nullable: false),
                    KepalaSekolah = table.Column<string>(type: "text", nullable: false),
                    NoHP = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformasiSekolah", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InformasiSekolah");
        }
    }
}
