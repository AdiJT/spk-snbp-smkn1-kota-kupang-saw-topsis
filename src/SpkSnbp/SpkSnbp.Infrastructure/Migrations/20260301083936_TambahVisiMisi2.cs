using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpkSnbp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TambahVisiMisi2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "InformasiSekolah",
                columns: new[] { "Id", "Akreditasi", "BentukPendidikan", "DesaKelurahan", "Jalan", "KabupatenKota", "KecamatanDistrik", "KepalaSekolah", "KodePos", "Misi", "NPSN", "NamaSekolah", "Nilai", "NoHP", "NoSKAkreditasi", "Provinsi", "TMTMulaiSKAkreditasi", "TMTSelesaiSKAkreditasi", "TanggalSKAkreditasi", "Visi" },
                values: new object[] { 1, "A", "SMK", "Oetete", "JL. PROF. DR. W. Z. JOHANES", "Kota Kupang", "Kec. Oebobo", "Mixon Rudolf Nicolas Abineno", "85112", "<ol>\r\n                <li>Menumbuhkan semangat keunggulan dan komparatif secara intensif kepada seluruh warga sekolah. </li>\r\n                <li>Melaksanakan kegiatan belajar mengajar secara optimal yang berorientasi kepada pencapaian kompetensi berstandar nasional/internasional dengan tetap mempertimbangkan potensi yang dimiliki peserta didik. </li>\r\n                <li>Mengembangkan dan mengintensifkan hubungan sekolah dengan DUDI dan institusi lain yang telah memiliki reputasi nasional/internasional sebagai perwujudan dari <em>demand driven</em>.</li>\r\n                <li>Menerapkan pengelolaan manajemen SMK mengacu pada standar ISO 9001 2008 dengan melibatkan seluruh warga sekolah dan <em>pelanggan</em>. </li>\r\n            </ol>", "50304998", "SMKN 1 Kota Kupang", 92.0, "081313474144", "1857/BAN-SM/SK/2022", "Prov. Nusa Tenggara Timur", new DateOnly(2022, 11, 30), new DateOnly(2027, 12, 31), new DateOnly(2022, 11, 30), "<p>\r\n                ”Menjadi Lembaga Pendidikan dan Pelatihan Bisnis Manajemen, Pariwisata, dan Teknologi Informatika dan Komunikasi Berstandar Nasional dan Internasional serta Mampu Menghasilkan Tamatan yang Berdaya Saing di 	Era Global.”\r\n            </p>" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InformasiSekolah",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
