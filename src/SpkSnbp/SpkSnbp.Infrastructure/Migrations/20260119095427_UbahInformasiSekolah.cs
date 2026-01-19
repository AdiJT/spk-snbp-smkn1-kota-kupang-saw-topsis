using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UbahInformasiSekolah : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DesaKelurahan",
                table: "InformasiSekolah",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Jalan",
                table: "InformasiSekolah",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KabupatenKota",
                table: "InformasiSekolah",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KecamatanDistrik",
                table: "InformasiSekolah",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KodePos",
                table: "InformasiSekolah",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Provinsi",
                table: "InformasiSekolah",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesaKelurahan",
                table: "InformasiSekolah");

            migrationBuilder.DropColumn(
                name: "Jalan",
                table: "InformasiSekolah");

            migrationBuilder.DropColumn(
                name: "KabupatenKota",
                table: "InformasiSekolah");

            migrationBuilder.DropColumn(
                name: "KecamatanDistrik",
                table: "InformasiSekolah");

            migrationBuilder.DropColumn(
                name: "KodePos",
                table: "InformasiSekolah");

            migrationBuilder.DropColumn(
                name: "Provinsi",
                table: "InformasiSekolah");
        }
    }
}
