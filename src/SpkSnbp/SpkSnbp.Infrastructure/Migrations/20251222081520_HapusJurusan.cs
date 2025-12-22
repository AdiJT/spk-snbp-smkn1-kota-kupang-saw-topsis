using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HapusJurusan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Siswa_Jurusan_JurusanId",
                table: "Siswa");

            migrationBuilder.DropTable(
                name: "Jurusan");

            migrationBuilder.DropIndex(
                name: "IX_Siswa_JurusanId",
                table: "Siswa");

            migrationBuilder.RenameColumn(
                name: "JurusanId",
                table: "Siswa",
                newName: "Jurusan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Jurusan",
                table: "Siswa",
                newName: "JurusanId");

            migrationBuilder.CreateTable(
                name: "Jurusan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nama = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jurusan", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Siswa_JurusanId",
                table: "Siswa",
                column: "JurusanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Siswa_Jurusan_JurusanId",
                table: "Siswa",
                column: "JurusanId",
                principalTable: "Jurusan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
