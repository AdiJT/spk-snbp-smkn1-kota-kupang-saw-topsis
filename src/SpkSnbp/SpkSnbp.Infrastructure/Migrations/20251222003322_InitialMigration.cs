using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Kriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nama = table.Column<string>(type: "text", nullable: false),
                    Bobot = table.Column<int>(type: "integer", nullable: false),
                    Jenis = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kriteria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TahunAjaran",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TahunAjaran", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Siswa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NISN = table.Column<string>(type: "text", nullable: false),
                    Nama = table.Column<string>(type: "text", nullable: false),
                    NilaiTopsis = table.Column<double>(type: "double precision", nullable: true),
                    Eligible = table.Column<int>(type: "integer", nullable: true),
                    TahunAjaranId = table.Column<int>(type: "integer", nullable: false),
                    JurusanId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Siswa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Siswa_Jurusan_JurusanId",
                        column: x => x.JurusanId,
                        principalTable: "Jurusan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Siswa_TahunAjaran_TahunAjaranId",
                        column: x => x.TahunAjaranId,
                        principalTable: "TahunAjaran",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiswaKriteria",
                columns: table => new
                {
                    IdSiswa = table.Column<int>(type: "integer", nullable: false),
                    IdKriteria = table.Column<int>(type: "integer", nullable: false),
                    Nilai = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiswaKriteria", x => new { x.IdSiswa, x.IdKriteria });
                    table.ForeignKey(
                        name: "FK_SiswaKriteria_Kriteria_IdKriteria",
                        column: x => x.IdKriteria,
                        principalTable: "Kriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiswaKriteria_Siswa_IdSiswa",
                        column: x => x.IdSiswa,
                        principalTable: "Siswa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Kriteria",
                columns: new[] { "Id", "Bobot", "Jenis", "Nama" },
                values: new object[,]
                {
                    { 1, 5, 0, "Mata Pelajaran Kejuruan" },
                    { 2, 4, 0, "Mata Pelajaran Umum" },
                    { 3, 4, 0, "Sertifikat LSP" },
                    { 4, 3, 0, "Sertifikat TKA" },
                    { 5, 2, 0, "Ekstrakurikuler" },
                    { 6, 1, 1, "Absensi" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "PasswordHash", "Role", "UserName" },
                values: new object[,]
                {
                    { 1, "AQAAAAIAAYagAAAAEKXsR8woVHO5DgmyBgmfe5b4I7jeJZYtk71JFY4HkDSCsimeHtIwzOueTyHo8gBH/A==", "Admin", "Admin" },
                    { 2, "AQAAAAIAAYagAAAAEKXsR8woVHO5DgmyBgmfe5b4I7jeJZYtk71JFY4HkDSCsimeHtIwzOueTyHo8gBH/A==", "WaliKelas", "Wali Kelas" },
                    { 3, "AQAAAAIAAYagAAAAEKXsR8woVHO5DgmyBgmfe5b4I7jeJZYtk71JFY4HkDSCsimeHtIwzOueTyHo8gBH/A==", "KepalaSekolah", "Kepala Sekolah" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Siswa_JurusanId",
                table: "Siswa",
                column: "JurusanId");

            migrationBuilder.CreateIndex(
                name: "IX_Siswa_TahunAjaranId",
                table: "Siswa",
                column: "TahunAjaranId");

            migrationBuilder.CreateIndex(
                name: "IX_SiswaKriteria_IdKriteria",
                table: "SiswaKriteria",
                column: "IdKriteria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiswaKriteria");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Kriteria");

            migrationBuilder.DropTable(
                name: "Siswa");

            migrationBuilder.DropTable(
                name: "Jurusan");

            migrationBuilder.DropTable(
                name: "TahunAjaran");
        }
    }
}
