using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SpkSnbp.Domain.Auth;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Web.Models.MataPelajaran;
using SpkSnbp.Web.Services.Toastr;
using System.Threading.Tasks;

namespace SpkSnbp.Web.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class MataPelajaranController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly IKriteriaRepository _kriteriaRepository;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly ISiswaKriteriaRepository _siswaKriteriaRepository;

    public MataPelajaranController(
        ISiswaRepository siswaRepository,
        IKriteriaRepository kriteriaRepository,
        ITahunAjaranRepository tahunAjaranRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        ISiswaKriteriaRepository siswaKriteriaRepository)
    {
        _siswaRepository = siswaRepository;
        _kriteriaRepository = kriteriaRepository;
        _tahunAjaranRepository = tahunAjaranRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _siswaKriteriaRepository = siswaKriteriaRepository;
    }

    public async Task<IActionResult> Index(Jurusan? jurusan = null, int? tahun = null)
    {
        var tahunAjaran = tahun is null ? null : await _tahunAjaranRepository.Get(tahun.Value);
        if (tahunAjaran is null)
            return View(new IndexVM { Jurusan = jurusan, DaftarEntry = (await _siswaRepository.GetAll(jurusan)).ToIndexEntryList() });

        return View(new IndexVM
        {
            Tahun = tahun,
            TahunAjaran = tahunAjaran,
            Jurusan = jurusan,
            DaftarEntry = (await _siswaRepository.GetAll(jurusan, tahun)).ToIndexEntryList()
        });
    }

    public async Task<IActionResult> Simpan(IndexVM vm)
    {
        foreach (var entry in vm.DaftarEntry)
        {
            var siswa = await _siswaRepository.Get(entry.IdSiswa);
            if (siswa is null) continue;

            var siswaKriteriaMPKejuruan = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPKejuruan);

            if (siswaKriteriaMPKejuruan is null)
            {
                siswaKriteriaMPKejuruan = new SiswaKriteria
                {
                    IdSiswa = siswa.Id,
                    IdKriteria = (int)KriteriaEnum.MPKejuruan,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteriaMPKejuruan);
            }

            siswaKriteriaMPKejuruan.Nilai = entry.MataPelajaranKejuruan;

            var siswaKriteriaMPUmum = siswa.DaftarSiswaKriteria.FirstOrDefault(x => x.IdKriteria == (int)KriteriaEnum.MPUmum);
            if (siswaKriteriaMPUmum is null)
            {
                siswaKriteriaMPUmum = new SiswaKriteria
                {
                    IdSiswa = siswa.Id,
                    IdKriteria = (int)KriteriaEnum.MPUmum,
                    Nilai = default
                };

                _siswaKriteriaRepository.Add(siswaKriteriaMPUmum);
            }

            siswaKriteriaMPUmum.Nilai = entry.MataPelajaranUmum;
        }

        var result = await _unitOfWork.SaveChangesAsync();
        if (result.IsSuccess)
            _notificationService.AddSuccess("Simpan Berhasil");
        else
            _notificationService.AddError("Simpan Gagal");

        return RedirectToActionPermanent(nameof(Index), new { vm.Jurusan, vm.Tahun });
    }
}
