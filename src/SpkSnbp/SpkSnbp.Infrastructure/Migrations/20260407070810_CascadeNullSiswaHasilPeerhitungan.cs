using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeNullSiswaHasilPeerhitungan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Siswa_HasilPerhitungan_HasilPerhitunganId",
                table: "Siswa");

            migrationBuilder.AddForeignKey(
                name: "FK_Siswa_HasilPerhitungan_HasilPerhitunganId",
                table: "Siswa",
                column: "HasilPerhitunganId",
                principalTable: "HasilPerhitungan",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Siswa_HasilPerhitungan_HasilPerhitunganId",
                table: "Siswa");

            migrationBuilder.AddForeignKey(
                name: "FK_Siswa_HasilPerhitungan_HasilPerhitunganId",
                table: "Siswa",
                column: "HasilPerhitunganId",
                principalTable: "HasilPerhitungan",
                principalColumn: "Id");
        }
    }
}
