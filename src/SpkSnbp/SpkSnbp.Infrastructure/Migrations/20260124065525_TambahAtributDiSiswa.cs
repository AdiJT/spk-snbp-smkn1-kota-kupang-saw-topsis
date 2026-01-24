using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TambahAtributDiSiswa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Ekstrakulikuler1",
                table: "Siswa",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ekstrakulikuler2",
                table: "Siswa",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ekstrakulikuler3",
                table: "Siswa",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JumlahAbsen",
                table: "Siswa",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SkorTKA",
                table: "Siswa",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ekstrakulikuler1",
                table: "Siswa");

            migrationBuilder.DropColumn(
                name: "Ekstrakulikuler2",
                table: "Siswa");

            migrationBuilder.DropColumn(
                name: "Ekstrakulikuler3",
                table: "Siswa");

            migrationBuilder.DropColumn(
                name: "JumlahAbsen",
                table: "Siswa");

            migrationBuilder.DropColumn(
                name: "SkorTKA",
                table: "Siswa");
        }
    }
}
