using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Razor.Templating.Core;
using SpkSnbp.Domain.Contracts;
using SpkSnbp.Domain.ModulUtama;
using SpkSnbp.Domain.Shared;
using SpkSnbp.Infrastructure.Services.FileServices;
using SpkSnbp.Web.Areas.Dashboard.Models;
using SpkSnbp.Web.Services.PDFGenerator;
using SpkSnbp.Web.Services.Toastr;

namespace SpkSnbp.Web.Areas.Dashboard.Controllers;

public class NilaiKriteriaController : Controller
{
    private readonly ISiswaRepository _siswaRepository;
    private readonly IKriteriaRepository _kriteriaRepository;
    private readonly ITahunAjaranRepository _tahunAjaranRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IToastrNotificationService _notificationService;
    private readonly ISiswaKriteriaRepository _siswaKriteriaRepository;
    private readonly IFileService _fileService;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IRazorTemplateEngine _templateEngine;
    private readonly IPDFGeneratorService _pDFGeneratorService;
    private readonly IKelasRepository _kelasRepository;

    public NilaiKriteriaController(
        ISiswaRepository siswaRepository,
        IKriteriaRepository kriteriaRepository,
        ITahunAjaranRepository tahunAjaranRepository,
        IUnitOfWork unitOfWork,
        IToastrNotificationService notificationService,
        ISiswaKriteriaRepository siswaKriteriaRepository,
        IFileService fileService,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IRazorTemplateEngine templateEngine,
        IPDFGeneratorService pDFGeneratorService,
        IKelasRepository kelasRepository)
    {
        _siswaRepository = siswaRepository;
        _kriteriaRepository = kriteriaRepository;
        _tahunAjaranRepository = tahunAjaranRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _siswaKriteriaRepository = siswaKriteriaRepository;
        _fileService = fileService;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _templateEngine = templateEngine;
        _pDFGeneratorService = pDFGeneratorService;
        _kelasRepository = kelasRepository;
    }

    public async Task<IActionResult> Index(
        int? id,
        Jurusan jurusan = Jurusan.TJKT,
        int? tahun = null,
        int? idKelas = null,
        bool first = true)
    {
        var tempDataDict = _tempDataDictionaryFactory.GetTempData(HttpContext);

        if (first)
        {
            var jurusanTempData = tempDataDict.Peek(TempDataKeys.Jurusan);
            var tahunTempData = tempDataDict.Peek(TempDataKeys.Tahun);
            var kelasTempData = tempDataDict.Peek(TempDataKeys.Kelas);
            var kriteriaTempData = tempDataDict.Peek(TempDataKeys.NilaiKriteria);

            if (jurusanTempData is not null)
                jurusan = (Jurusan)jurusanTempData;

            if (tahunTempData is not null)
                tahun = (int)tahunTempData;

            if (kelasTempData is not null)
                idKelas = (int)kelasTempData;

            if (kriteriaTempData is not null)
                id = (int)kriteriaTempData;
        }
        else
            tempDataDict[TempDataKeys.Jurusan] = jurusan;

        var tahunAjaran = tahun is null ?
                    await _tahunAjaranRepository.Get(CultureInfos.DateOnlyNow.Year) :
                    await _tahunAjaranRepository.Get(tahun.Value);

        tahunAjaran ??= await _tahunAjaranRepository.GetLatest();

        var kelas = idKelas is null ? null : await _kelasRepository.Get(idKelas.Value);

        var kriteria = id is null ? null : await _kriteriaRepository.Get(id.Value);

        if (!first && tahunAjaran is not null) tempDataDict[TempDataKeys.Tahun] = tahunAjaran.Id;
        if (!first) tempDataDict[TempDataKeys.Kelas] = kelas?.Id;
        if (!first) tempDataDict[TempDataKeys.NilaiKriteria] = kriteria?.Id;

        return View();
    }
}
