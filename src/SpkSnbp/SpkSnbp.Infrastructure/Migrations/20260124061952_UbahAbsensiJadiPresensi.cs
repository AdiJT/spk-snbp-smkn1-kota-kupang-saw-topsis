using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UbahAbsensiJadiPresensi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Kriteria",
                keyColumn: "Id",
                keyValue: 6,
                column: "Nama",
                value: "Presensi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Kriteria",
                keyColumn: "Id",
                keyValue: 6,
                column: "Nama",
                value: "Absensi");
        }
    }
}
