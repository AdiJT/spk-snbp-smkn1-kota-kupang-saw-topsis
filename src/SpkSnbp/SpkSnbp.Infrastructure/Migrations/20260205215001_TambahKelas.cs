using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TambahKelas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KelasId",
                table: "Siswa",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Kelas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nama = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kelas", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Kelas",
                columns: new[] { "Id", "Nama" },
                values: new object[,]
                {
                    { 1, "1" },
                    { 2, "2" },
                    { 3, "3" },
                    { 4, "4" },
                    { 5, "5" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Siswa_KelasId",
                table: "Siswa",
                column: "KelasId");

            migrationBuilder.AddForeignKey(
                name: "FK_Siswa_Kelas_KelasId",
                table: "Siswa",
                column: "KelasId",
                principalTable: "Kelas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Siswa_Kelas_KelasId",
                table: "Siswa");

            migrationBuilder.DropTable(
                name: "Kelas");

            migrationBuilder.DropIndex(
                name: "IX_Siswa_KelasId",
                table: "Siswa");

            migrationBuilder.DropColumn(
                name: "KelasId",
                table: "Siswa");
        }
    }
}
