using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UbahRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "Role",
                value: "Wali Kelas");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "Role",
                value: "Kepala Sekolah");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "Role",
                value: "WaliKelas");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                column: "Role",
                value: "KepalaSekolah");
        }
    }
}
