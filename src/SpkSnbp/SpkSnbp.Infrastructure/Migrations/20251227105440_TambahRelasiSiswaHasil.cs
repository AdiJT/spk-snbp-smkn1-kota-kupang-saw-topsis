using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TambahRelasiSiswaHasil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HasilPerhitunganId",
                table: "Siswa",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Siswa_HasilPerhitunganId",
                table: "Siswa",
                column: "HasilPerhitunganId");

            migrationBuilder.AddForeignKey(
                name: "FK_Siswa_HasilPerhitungan_HasilPerhitunganId",
                table: "Siswa",
                column: "HasilPerhitunganId",
                principalTable: "HasilPerhitungan",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Siswa_HasilPerhitungan_HasilPerhitunganId",
                table: "Siswa");

            migrationBuilder.DropIndex(
                name: "IX_Siswa_HasilPerhitunganId",
                table: "Siswa");

            migrationBuilder.DropColumn(
                name: "HasilPerhitunganId",
                table: "Siswa");
        }
    }
}
