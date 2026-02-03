namespace SpkSnbp.Web.Services.PDFGenerator;

public interface IPDFGeneratorService
{
    Task<byte[]> GeneratePDF(
        string html,
        double marginTop,
        double marginBottom,
        double marginLeft,
        double marginRight);
}
