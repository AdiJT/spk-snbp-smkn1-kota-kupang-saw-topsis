using Microsoft.AspNetCore.Http;
using SpkSnbp.Domain.Shared;

namespace SpkSnbp.Infrastructure.Services.FileServices;

public interface IFileService
{
    bool IsExist(Uri uri);
    Result Delete(Uri uri);
    Task<Result<Uri>> UploadFile<TModel>(IFormFile formFile, string folderPath, string[] permittedExtension, long minSizeLimit, long maxSizeLimit);
    Task<Result<byte[]>> ProcessFormFile<TModel>(IFormFile formFile, string[] permittedExtensions, long minSizeLimit, long maxSizeLimit);
}
