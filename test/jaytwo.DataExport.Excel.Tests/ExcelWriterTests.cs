using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using jaytwo.DataExport.Excel.Styles;
using jaytwo.DataExport.Excel.Tests.Models;
using jaytwo.DisappearingFiles;
using jaytwo.RuntimeRevelation;
using Xunit;
using Xunit.Abstractions;

namespace jaytwo.DataExport.Excel.Tests;

public class ExcelWriterTests
{
    private readonly ITestOutputHelper _output;

    public ExcelWriterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task WriteData_WritesExpectedRows()
    {
        // Arrange
        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
        };

        using var memoryStream = new MemoryStream();
        using (var exporter = new ExcelWriter(memoryStream))
        {
            // Act
            await exporter.WriteSheetAsync(people);
        }

        memoryStream.Position = 0; // Reset to beginning for reading

        // Assert
        using var doc = SpreadsheetDocument.Open(memoryStream, isEditable: false);
        var sheet = doc.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().First();
        var worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(sheet.Id!);
        var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

        var rows = sheetData.Elements<Row>().ToList();
        Assert.Equal(3, rows.Count); // 1 header + 2 data rows

        var headerCells = rows[0].Elements<Cell>().ToList();
        Assert.Equal("Name", GetCellValue(doc, headerCells[0]));
        Assert.Equal("Age", GetCellValue(doc, headerCells[1]));

        var row1 = rows[1].Elements<Cell>().ToList();
        Assert.Equal("Alice", GetCellValue(doc, row1[0]));
        Assert.Equal("30", GetCellValue(doc, row1[1]));

        var row2 = rows[2].Elements<Cell>().ToList();
        Assert.Equal("Bob", GetCellValue(doc, row2[0]));
        Assert.Equal("25", GetCellValue(doc, row2[1]));

        WriteMemoryUsage();
        WriteFileSize(memoryStream.Length);
    }

    [Fact]
    public async Task Write_multiple_sheets_WritesExpectedRows()
    {
        // Arrange
        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 },
        };

        var pets = new List<Pet>
        {
            new Pet { Name = "Mickey", Animal = "Mouse" },
            new Pet { Name = "Bugs", Animal = "Bunny" },
        };

        using var memoryStream = new MemoryStream();
        using (var exporter = new ExcelWriter(memoryStream))
        {
            // Act
            await exporter.WriteSheetAsync(people, "People");
            await exporter.WriteSheetAsync(pets, "Pets");
        }

        memoryStream.Position = 0; // Reset to beginning for reading

        // Assert
        using var doc = SpreadsheetDocument.Open(memoryStream, false);
        var peopleSheet = doc.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().First();
        var peopleWorksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(peopleSheet.Id!);
        var peopleSheetData = peopleWorksheetPart.Worksheet.Elements<SheetData>().First();

        var petsSheet = doc.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().Last();
        var petsWorksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(petsSheet.Id!);
        var petsSheetData = petsWorksheetPart.Worksheet.Elements<SheetData>().First();

        var peopleRows = peopleSheetData.Elements<Row>().ToList();
        Assert.Equal(3, peopleRows.Count); // 1 header + 2 data rows

        var peopleHeaderCells = peopleRows[0].Elements<Cell>().ToList();
        Assert.Equal("Name", GetCellValue(doc, peopleHeaderCells[0]));
        Assert.Equal("Age", GetCellValue(doc, peopleHeaderCells[1]));

        var peopleRow1 = peopleRows[1].Elements<Cell>().ToList();
        Assert.Equal("Alice", GetCellValue(doc, peopleRow1[0]));
        Assert.Equal("30", GetCellValue(doc, peopleRow1[1]));

        var peopleRow2 = peopleRows[2].Elements<Cell>().ToList();
        Assert.Equal("Bob", GetCellValue(doc, peopleRow2[0]));
        Assert.Equal("25", GetCellValue(doc, peopleRow2[1]));

        var petsRows = petsSheetData.Elements<Row>().ToList();
        Assert.Equal(3, petsRows.Count); // 1 header + 2 data rows

        var petsHeaderCells = petsRows[0].Elements<Cell>().ToList();
        Assert.Equal("Name", GetCellValue(doc, petsHeaderCells[0]));
        Assert.Equal("Animal", GetCellValue(doc, petsHeaderCells[1]));

        var petsRow1 = petsRows[1].Elements<Cell>().ToList();
        Assert.Equal("Mickey", GetCellValue(doc, petsRow1[0]));
        Assert.Equal("Mouse", GetCellValue(doc, petsRow1[1]));

        var petsRow2 = petsRows[2].Elements<Cell>().ToList();
        Assert.Equal("Bugs", GetCellValue(doc, petsRow2[0]));
        Assert.Equal("Bunny", GetCellValue(doc, petsRow2[1]));

        WriteMemoryUsage();
        WriteFileSize(memoryStream.Length);
    }

    [Theory]
    [InlineData(1000)]
    //[InlineData(10000)]
    //[InlineData(100000)]
    //[InlineData(200000)]
    //[InlineData(900000)]
    public async Task WritingToDisk(int personCount)
    {
        // Arrange
        var currentProcess = Process.GetCurrentProcess();
        var maxMemoryUsed = 0L;
        var maxManagedMemoryUsed = 0L;
        var peopleEnumerable = PersonFactory.GeneratePeople(personCount)
            .Select(x =>
            {
                maxMemoryUsed = Math.Max(maxMemoryUsed, currentProcess.WorkingSet64);
                maxManagedMemoryUsed = Math.Max(maxManagedMemoryUsed, GC.GetTotalMemory(false));
                return x;
            });

        using var temp = DisappearingDirectory.CreateInTempPath();
        var tempFile = temp.CreateNewFile("people.xlsx");
        using var fileStream = new DisappearingFileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        using (var exporter = new ExcelWriter(fileStream))
        {
            // Act
            await exporter.WriteSheetAsync(peopleEnumerable);
        }

        // Assert
        _output.WriteLine($"Max Memory Usage: {maxMemoryUsed / (1024.0 * 1024.0):F2} MB");
        _output.WriteLine($"Max Managed Memory: {maxManagedMemoryUsed / (1024.0 * 1024.0):F2} MB");

        WriteMemoryUsage();
        WriteFileSize(fileStream.Length);
    }

    [SkippableTheory]
    [InlineData(10, 1)]
    [InlineData(500, 5)] // TODO: figure out why it wants me to save after opening a file with multiple sheets
    public async Task SanityCheck_LaunchesExcel(int rows, int sheets)
    {
        Skip.IfNot(Debugger.IsAttached, "Not Debugging");
        Skip.If(RuntimeInformation.Current.Platform != OSPlatform.Windows, "Not Running on Windows");

        // Arrange
        var outputFileName = new SolutionResolution.SlnFileResolver().ResolvePathRelativeToSln($"out/{DateTime.Now.Ticks}.xlsx");
        using (var fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
        {
            // Act
            await BuildWorksheetSampleWorkSheet(fileStream, rowsPerSheet: rows, sheetCount: sheets);
        }

        Process.Start("explorer", outputFileName);

        // Assert
        WriteMemoryUsage();
        WriteFileSize(new FileInfo(outputFileName).Length);
    }

    private static string GetCellValue(SpreadsheetDocument document, Cell cell)
    {
        if (cell == null)
        {
            return string.Empty;
        }

        var value = cell.CellValue?.InnerText ?? string.Empty;

        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var index = int.Parse(value);
            return document.WorkbookPart!.SharedStringTablePart!.SharedStringTable.Elements<SharedStringItem>().ElementAt(index).InnerText;
        }

        return value;
    }

    private void WriteMemoryUsage()
    {
        var currentProcess = Process.GetCurrentProcess();
        long memoryInBytes = currentProcess.WorkingSet64;
        _output.WriteLine($"Memory Usage: {memoryInBytes / (1024.0 * 1024.0):F2} MB");
        _output.WriteLine($"Managed Memory: {GC.GetTotalMemory(false) / (1024.0 * 1024.0):F2} MB");
    }

    private void WriteFileSize(long length)
    {
        _output.WriteLine($"Stream Size: {length / (1024.0 * 1024.0):F2} MB");
    }

    private async Task BuildWorksheetSampleWorkSheet(Stream outputStream, int rowsPerSheet = 10, int sheetCount = 2)
    {
        using (var exporter = new ExcelWriter(outputStream))
        {
            for (var i = 0; i < sheetCount; i++)
            {
                var options = new WorksheetOptions(applyZebraStripe: true, freezeHeaderRow: true, boldHeaderRow: true)
                    .SetupColumn("Name", new() { Width = 30 })
                    .SetupColumn("Age", new() { Width = 7, HorizontalAlignment = HorizontalAlignmentStyles.Center })
                    .SetupColumn("Biography", new() { Width = 50 })
                    .SetupColumn("Lipsum", new() { Width = 50 })
                    .SetupColumn("BirthDate", new() { Width = 20 });

                await exporter.WriteSheetAsync(
                    PersonFactory.GeneratePeople(rowsPerSheet),
                    $"People{i}",
                    options);
            }
        }
    }
}
