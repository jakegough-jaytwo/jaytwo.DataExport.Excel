using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using jaytwo.DisappearingFiles;
using jaytwo.StreamingExcelExport.Tests.Models;
using Xunit;
using Xunit.Abstractions;

namespace jaytwo.StreamingExcelExport.Tests;

public class StreamingExcelExporterTests
{
    private readonly ITestOutputHelper _output;

    public StreamingExcelExporterTests(ITestOutputHelper output)
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
        var exporter = new StreamingExcelExporter<Person>();

        // Act
        await exporter.WriteDataAsync(memoryStream, people);
        memoryStream.Position = 0; // Reset to beginning for reading

        // Assert
        using var doc = SpreadsheetDocument.Open(memoryStream, false);
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

    [Theory]
    [InlineData(10000)]
    [InlineData(100000)]
    [InlineData(200000)]
    [InlineData(900000)]
    public async Task WritingToDisk(int personCount)
    {
        // Arrange
        var peopleEnumerable = PersonFactory.GeneratePeople(personCount);

        using var temp = DisappearingDirectory.CreateInTempPath();
        var tempFile = temp.CreateNewFile("people.xlsx");
        using var fileStream = new DisappearingFileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        var exporter = new StreamingExcelExporter<Person>();

        // Act
        await exporter.WriteDataAsync(fileStream, peopleEnumerable);

        // Assert

        WriteMemoryUsage();
        WriteFileSize(fileStream.Length);
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
}
