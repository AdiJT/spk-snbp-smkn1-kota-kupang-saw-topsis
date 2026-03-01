using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TambahVisiMis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Misi",
                table: "InformasiSekolah",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Visi",
                table: "InformasiSekolah",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Misi",
                table: "InformasiSekolah");

            migrationBuilder.DropColumn(
                name: "Visi",
                table: "InformasiSekolah");
        }
    }
}
