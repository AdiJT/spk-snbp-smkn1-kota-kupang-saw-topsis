using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace SpkSnbp.Web.Services.PDFGenerator;

public class PDFGeneratorService : IPDFGeneratorService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private bool _browserFetched = false;

    public PDFGeneratorService(IWebHostEnvironment environment)
    {
        _webHostEnvironment = environment;
    }

    public async Task<byte[]> GeneratePDF(
        string html,
        double marginTop,
        double marginBottom,
        double marginLeft,
        double marginRight)
    {
        if (!_browserFetched)
        {
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
        }

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Timeout = 0, Headless = true, Args = ["--no-sandbox"] });
        using var page = await browser.NewPageAsync();
        await page.SetContentAsync(html);

        return await page.PdfDataAsync(new PdfOptions
        {
            DisplayHeaderFooter = false,
            Format = PaperFormat.A4,
            HeaderTemplate = "",
            FooterTemplate = "",
            MarginOptions = new MarginOptions
            {
                Top = $"{marginTop}px",
                Bottom = $"{marginBottom}px",
                Left = $"{marginLeft}px",
                Right = $"{marginRight}px",
            }
        });
    }
}
