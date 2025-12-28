using DocumentFormat.OpenXml.Spreadsheet;

namespace SpkSnbp.Web.Helpers;

public static class HelperFunctions
{
    public static string GetCellValues(Cell cell, List<string> sharedStrings)
    {
        if (cell.CellFormula != null)
            return cell.CellValue!.InnerText;

        if (cell.DataType is null) return cell.InnerText;

        return sharedStrings[int.Parse(cell.InnerText)];
    }
}
