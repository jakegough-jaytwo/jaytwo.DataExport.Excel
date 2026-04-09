using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using jaytwo.DataExport.Abstractions;
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

    [Fact]
    public async Task WriteData_WithMismatchedColumnDefinitionCase_AppliesColumnStyles()
    {
        // Arrange — column definition key casing doesn't match the property name casing
        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30 },
        };

        var options = new WorksheetOptions()
            .SetupColumn("name", new ColumnDefinition { Width = 25, HorizontalAlignment = HorizontalAlignmentStyles.Center }) // lowercase 'n', property is 'Name'
            .SetupColumn("AGE", new ColumnDefinition { Width = 10 }); // all-caps, property is 'Age'

        using var memoryStream = new MemoryStream();
        using (var exporter = new ExcelWriter(memoryStream))
        {
            await exporter.WriteSheetAsync(people, sheetOptions: options);
        }

        memoryStream.Position = 0;

        // Assert — the file is valid and column widths are applied
        using var doc = SpreadsheetDocument.Open(memoryStream, isEditable: false);
        var sheet = doc.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().First();
        var worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(sheet.Id!);

        var cols = worksheetPart.Worksheet.Elements<DocumentFormat.OpenXml.Spreadsheet.Columns>().FirstOrDefault();
        Assert.NotNull(cols);

        var colList = cols!.Elements<Column>().ToList();
        Assert.Equal(2, colList.Count);

        // Column 1 = Name, width 25
        Assert.Equal(1U, colList[0].Min?.Value);
        Assert.Equal(25d, colList[0].Width?.Value);

        // Column 2 = Age, width 10
        Assert.Equal(2U, colList[1].Min?.Value);
        Assert.Equal(10d, colList[1].Width?.Value);

        WriteMemoryUsage();
        WriteFileSize(memoryStream.Length);
    }

    [Fact]
    public async Task WriteData_WithCaseSensitiveColumnDefinitionsAssignedThroughSetter_AppliesColumnStyles()
    {
        // Arrange
        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30 },
        };

        var options = new WorksheetOptions
        {
            ColumnDefinitions = new Dictionary<string, ColumnDefinition>
            {
                ["name"] = new ColumnDefinition { Width = 25, HorizontalAlignment = HorizontalAlignmentStyles.Center },
                ["AGE"] = new ColumnDefinition { Width = 10 },
            },
        };

        using var memoryStream = new MemoryStream();
        using (var exporter = new ExcelWriter(memoryStream))
        {
            await exporter.WriteSheetAsync(people, sheetOptions: options);
        }

        memoryStream.Position = 0;

        // Assert
        using var doc = SpreadsheetDocument.Open(memoryStream, isEditable: false);
        var sheet = doc.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().First();
        var worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(sheet.Id!);

        var cols = worksheetPart.Worksheet.Elements<DocumentFormat.OpenXml.Spreadsheet.Columns>().FirstOrDefault();
        Assert.NotNull(cols);

        var colList = cols!.Elements<Column>().ToList();
        Assert.Equal(2, colList.Count);
        Assert.Equal(1U, colList[0].Min?.Value);
        Assert.Equal(25d, colList[0].Width?.Value);
        Assert.Equal(2U, colList[1].Min?.Value);
        Assert.Equal(10d, colList[1].Width?.Value);
    }

    [Fact]
    public async Task WriteData_FromDataReader_WritesExpectedRows()
    {
        // Arrange
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Age", typeof(int));
        table.Rows.Add("Alice", 30);
        table.Rows.Add("Bob", 25);

        using var memoryStream = new MemoryStream();
        using (var exporter = new ExcelWriter(memoryStream))
        {
            // Act
            await exporter.WriteSheetAsync(table.CreateDataReader());
        }

        memoryStream.Position = 0;

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
    public async Task WriteData_FromTabularDataReader_WritesExpectedRows()
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
            await exporter.WriteSheetAsync(new ObjectTabularDataReader<Person>(people));
        }

        memoryStream.Position = 0;

        // Assert
        using var doc = SpreadsheetDocument.Open(memoryStream, isEditable: false);
        var sheet = doc.WorkbookPart!.Workbook.Sheets!.Elements<Sheet>().First();
        var worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(sheet.Id!);
        var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

        var rows = sheetData.Elements<Row>().ToList();
        Assert.Equal(3, rows.Count);

        var headerCells = rows[0].Elements<Cell>().ToList();
        Assert.Equal("Name", GetCellValue(doc, headerCells[0]));
        Assert.Equal("Age", GetCellValue(doc, headerCells[1]));

        var row1 = rows[1].Elements<Cell>().ToList();
        Assert.Equal("Alice", GetCellValue(doc, row1[0]));
        Assert.Equal("30", GetCellValue(doc, row1[1]));

        var row2 = rows[2].Elements<Cell>().ToList();
        Assert.Equal("Bob", GetCellValue(doc, row2[0]));
        Assert.Equal("25", GetCellValue(doc, row2[1]));
    }

    [Fact]
    public async Task WriteMultipleFrozenSheets_DoesNotMarkEverySheetAsSelected()
    {
        // Arrange
        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30 },
        };

        var options = new WorksheetOptions(freezeHeaderRow: true);

        using var memoryStream = new MemoryStream();
        using (var exporter = new ExcelWriter(memoryStream))
        {
            await exporter.WriteSheetAsync(people, "People1", options);
            await exporter.WriteSheetAsync(people, "People2", options);
        }

        memoryStream.Position = 0;

        // Assert
        using var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Read, leaveOpen: true);
        var sheetEntries = archive.Entries
            .Where(x => x.FullName.StartsWith("xl/worksheets/", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.FullName)
            .ToList();

        Assert.Equal(2, sheetEntries.Count);

        foreach (var entry in sheetEntries)
        {
            using var stream = entry.Open();
            var document = XDocument.Load(stream);
            var sheetView = document
                .Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "sheetView");

            Assert.NotNull(sheetView);
            Assert.Null(sheetView!.Attribute("tabSelected"));
        }
    }

    [Fact]
    public async Task WriteMultipleSheets_AssignsDistinctWorksheetUids()
    {
        // Arrange
        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30 },
        };

        using var memoryStream = new MemoryStream();
        using (var exporter = new ExcelWriter(memoryStream))
        {
            await exporter.WriteSheetAsync(people, "People1");
            await exporter.WriteSheetAsync(people, "People2");
        }

        memoryStream.Position = 0;

        // Assert
        using var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Read, leaveOpen: true);
        var worksheetUids = archive.Entries
            .Where(x => x.FullName.StartsWith("xl/worksheets/", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.FullName)
            .Select(entry =>
            {
                using var stream = entry.Open();
                var document = XDocument.Load(stream);
                var worksheet = document.Root;
                return worksheet?.Attributes().FirstOrDefault(x => x.Name.LocalName == "uid")?.Value;
            })
            .ToList();

        Assert.Equal(2, worksheetUids.Count);
        Assert.All(worksheetUids, x => Assert.False(string.IsNullOrWhiteSpace(x)));
        Assert.Equal(2, worksheetUids.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task WriteData_WithMetadata_WritesCoreAndExtendedProperties()
    {
        // Arrange
        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30 },
        };

        var createdAt = new DateTime(2024, 01, 02, 03, 04, 05, DateTimeKind.Utc);
        var modifiedAt = new DateTime(2024, 06, 07, 08, 09, 10, DateTimeKind.Utc);
        var metadata = new WorkbookMetadata
        {
            Creator = "Cru Jones",
            LastModifiedBy = "Christian Hollings",
            Title = "Race Results",
            Subject = "Qualifier Heat",
            Description = "Exported rider standings",
            Keywords = "bmx,race,qualifier",
            Category = "Competition",
            CompanyName = "Rad Racing",
            ApplicationName = "BMXcel",
            ApplicationVersion = "1986.3.21",
            CreatedAtUtc = createdAt,
            ModifiedAtUtc = modifiedAt,
        };

        using var memoryStream = new MemoryStream();
        using (var exporter = new ExcelWriter(memoryStream, metadata))
        {
            // Act
            await exporter.WriteSheetAsync(people, "People");
        }

        memoryStream.Position = 0;

        // Assert
        using var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Read, leaveOpen: true);
        var coreProperties = LoadXml(archive, "docProps/core.xml");
        var extendedProperties = LoadXml(archive, "docProps/app.xml");

        Assert.Equal("Race Results", GetElementValue(coreProperties, "title"));
        Assert.Equal("Qualifier Heat", GetElementValue(coreProperties, "subject"));
        Assert.Equal("Exported rider standings", GetElementValue(coreProperties, "description"));
        Assert.Equal("Cru Jones", GetElementValue(coreProperties, "creator"));
        Assert.Equal("Christian Hollings", GetElementValue(coreProperties, "lastModifiedBy"));
        Assert.Equal("bmx,race,qualifier", GetElementValue(coreProperties, "keywords"));
        Assert.Equal("Competition", GetElementValue(coreProperties, "category"));
        Assert.Equal("2024-01-02T03:04:05Z", GetElementValue(coreProperties, "created"));
        Assert.Equal("2024-06-07T08:09:10Z", GetElementValue(coreProperties, "modified"));

        Assert.Equal("BMXcel", GetElementValue(extendedProperties, "Application"));
        Assert.Equal("1986.3.21", GetElementValue(extendedProperties, "AppVersion"));
        Assert.Equal("Rad Racing", GetElementValue(extendedProperties, "Company"));
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

#if NET6_0_OR_GREATER
    [SkippableTheory]
    [InlineData(10, 1)]
    [InlineData(10, 2)]
    [InlineData(500, 5)]
    public async Task SanityCheck_LaunchesExcel(int rows, int sheets)
    {
        Skip.IfNot(Debugger.IsAttached, "Not Debugging");
        Skip.If(RuntimeInformation.Current.Platform != OSPlatform.Windows, "Not Running on Windows");

        // Arrange
        var outputFileName = new SolutionResolution.SlnFileResolver().ResolvePathRelativeToSln($"out/{DateTime.Now.Ticks}.xlsx");
        using (var fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
        {
            // Act
            using (var exporter = new ExcelWriter(fileStream))
            {
                for (var i = 0; i < sheets; i++)
                {
                    var options = new WorksheetOptions(applyZebraStripe: true, freezeHeaderRow: true, boldHeaderRow: true)
                        .SetupColumn("Name", new() { Width = 30 })
                        .SetupColumn("Age", new() { Width = 7, HorizontalAlignment = HorizontalAlignmentStyles.Center })
                        .SetupColumn("Biography", new() { Width = 50 })
                        .SetupColumn("Lipsum", new() { Width = 50 })
                        .SetupColumn("BirthDate", new() { Width = 20 });

                    await exporter.WriteSheetAsync(
                        PersonFactory.GeneratePeople(rows),
                        $"People{i}",
                        options);
                }
            }
        }

        Process.Start("explorer", outputFileName);

        // Assert
        WriteMemoryUsage();
        WriteFileSize(new FileInfo(outputFileName).Length);
    }
#endif

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

    private static XDocument LoadXml(System.IO.Compression.ZipArchive archive, string entryPath)
    {
        var entry = archive.GetEntry(entryPath);
        Assert.NotNull(entry);

        using var stream = entry!.Open();
        return XDocument.Load(stream);
    }

    private static string? GetElementValue(XDocument document, string localName)
        => document.Root?
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == localName)?
            .Value;

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
