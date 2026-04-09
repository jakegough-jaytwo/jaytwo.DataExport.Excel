# jaytwo.DataExport.Excel

[![NuGet Version](https://img.shields.io/nuget/v/jaytwo.DataExport.Excel.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/jaytwo.DataExport.Excel)
[![NuGet Downloads](https://img.shields.io/nuget/dt/jaytwo.DataExport.Excel.svg?style=flat)](https://www.nuget.org/packages/jaytwo.DataExport.Excel)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://mit-license.org/)

A streaming XLSX writer for .NET focused on low memory usage for exporting large datasets without crashing your web server.

[View source on GitHub](https://github.com/jakegough-jaytwo/jaytwo.DataExport.Excel)

## Features

- Stream XLSX files directly to a writable stream with no in-memory workbook buildup
- Real `.xlsx` OpenXML format with XML parts written via `XmlWriter`
- ZIP file packaging done with a streaming ZIP implementation, so seekable streams are not required
- Async-first API supporting `DbDataReader`, `IAsyncEnumerable<T>`, and `IEnumerable<T>`
- Simple one-shot export methods for either a `Stream` or a file path
- Straightforward multi-sheet writing via a disposable `ExcelWriter`
- Metadata support for creator, app name, company, version, and creation timestamp
- Optional `WorksheetOptions` / `ColumnDefinition` for widths, alignment, frozen headers, zebra striping, and header formatting
- Small memory footprint, even for large datasets
- Ideal for use in ASP.NET, serverless functions, or background jobs

## Background

Life would be easier if users were ok with a CSV export. Sometimes users insist on a real Excel export, and they cannot be fooled by renaming a CSV or TSV to `.xls`.

An Excel file does come with a few useful creature comforts for data exports: compressed file size, custom column widths, frozen header row, and cell formatting including alignment, zebra striping, and bold headers.

There are already packages that handle straightforward Excel export. Those packages work great until the data gets big. `XLSX` is just a ZIP of XML files, and most libraries load all that XML into memory before writing the final archive. A million rows means tens of millions of DOM elements and a melted web server.

This library prioritizes low memory usage and does not depend on seekable streams. The full Excel file is streamed directly to the output and is never buffered entirely in memory or on disk.

### The Two Hurdles

1. **XML**: many libraries build full in-memory DOMs. This one uses `XmlWriter`, writing XML in a forward-only stream.
2. **ZIP**: .NET's `System.IO.Compression` requires seekable streams to write ZIPs. This library implements a custom ZIP32 writer to support fully streaming ZIP output.

> ZIP32 format is used for compatibility. It imposes a 4 GB limit per uncompressed XML document within the archive, for example `sheet1.xml`. ZIP64, which removes this limit, is not reliably supported by Excel when streamed, so this library deliberately avoids ZIP64 for now.

## Installation

Add the NuGet package:

```powershell
PM> Install-Package jaytwo.DataExport.Excel
```

## Usage

There are two intended usage patterns:

1. Use the static `ExcelWriter.ExportAsync(...)` methods for one-shot exports when you have tabular data and a destination.
2. Create an `ExcelWriter` instance and call `WriteSheetAsync(...)` multiple times when you want multiple sheets in a single workbook.

For object-based exports, public properties of `T` become columns. For tabular exports, you can also write directly from a `DbDataReader`.

> Warning: Be careful not to confuse `IEnumerable<T>` with concrete collections like `List<T>` or arrays (`T[]`).
>
> While `List<T>` and arrays do implement `IEnumerable<T>`, they are also `ICollection<T>`, meaning the entire dataset is already fully loaded into memory before you pass it to the writer. In these cases, the streaming benefits of this library are diminished or lost.
>
> To take advantage of streaming for large datasets, prefer `IAsyncEnumerable<T>`, `DbDataReader`, or a lazily-yielding `IEnumerable<T>` generated with `yield return`.

> Warning: While this package is designed to write directly to a stream, use caution when writing to an HTTP stream.
>
> If an exception occurs halfway through the export and you have already set the HTTP status code to `200 OK`, the client has no reliable way to detect that the response was incomplete or corrupted.

### One-Shot Export to a Stream

```csharp
await ExcelWriter.ExportAsync(outputStream, GetPeopleAsync(), sheetName: "People");
```

`ExportAsync` also supports `IEnumerable<T>` and `DbDataReader`:

```csharp
await ExcelWriter.ExportAsync(outputStream, people, sheetName: "People");
await ExcelWriter.ExportAsync(outputStream, dataReader, sheetName: "People");
```

### One-Shot Export to a File

```csharp
await ExcelWriter.ExportAsync("people.xlsx", GetPeopleAsync(), sheetName: "People");
```

File-path overloads are also available for `IEnumerable<T>` and `DbDataReader`.

### Adding Custom Metadata

```csharp
var radMetadata = new WorkbookMetadata
{
    Creator = "Cru Jones",
    LastModifiedBy = "Cru Jones",
    CompanyName = "Rad Racing",
    ApplicationName = "BMXcel",
    ApplicationVersion = "1986.3.21",
    Title = "People Export",
    Subject = "People",
    Description = "Generated by BMXcel",
    Keywords = "people export",
    Category = "exports",
    CreatedAtUtc = DateTime.UtcNow,
    ModifiedAtUtc = DateTime.UtcNow,
};

await ExcelWriter.ExportAsync(outputStream, GetPeopleAsync(), sheetName: "People", metadata: radMetadata);
```

### Writing Multiple Sheets to a Single Workbook

```csharp
await using (var excelWriter = new ExcelWriter(outputStream))
{
    await excelWriter.WriteSheetAsync(GetProductsAsync(), sheetName: "Products");
    await excelWriter.WriteSheetAsync(GetCustomersAsync(), sheetName: "Customers");
}
```

### Using Custom Sheet Options

All four `WorksheetOptions` constructor parameters default to `true`.

```csharp
var options = new WorksheetOptions(
        includeHeader: true,
        boldHeaderRow: true,
        freezeHeaderRow: true,
        applyZebraStripe: true)
    .SetupColumn("Name", new() { Width = 30 })
    .SetupColumn("Age", new() { Width = 7, HorizontalAlignment = HorizontalAlignmentStyles.Center })
    .SetupColumn("BirthDate", new() { Width = 20 });

await ExcelWriter.ExportAsync(outputStream, peopleData, sheetName: "People", sheetOptions: options);
```

### Formatting Numbers and Dates

#### Using `NumberFormat` (Excel built-in formats)

`NumberFormat` applies one of Excel's built-in numeric format codes. Excel interprets these using the viewer's locale at render time, so a date column will display differently on machines with different regional settings.

```csharp
var options = new WorksheetOptions()
    .SetupColumn("BirthDate", new() { NumberFormat = NumberFormatStyles.DateShort })
    .SetupColumn("Salary", new() { NumberFormat = NumberFormatStyles.CurrencyTwoDecimals })
    .SetupColumn("Score", new() { NumberFormat = NumberFormatStyles.NumberDecimalTwoPlaces });

await ExcelWriter.ExportAsync(outputStream, peopleData, sheetName: "People", sheetOptions: options);
```

Use `NumberFormat` when Excel's built-in formats are sufficient and you don't need the workbook to look identical across different regional settings.

#### Using `LocaleFormat` (explicit culture-specific format codes)

`LocaleFormat` causes this library to emit an explicit, culture-derived format code into the stylesheet rather than relying on Excel's built-in format interpretation. This ensures consistent rendering regardless of the viewer's regional settings.

Mixed-currency export where each column gets its own locale:

```csharp
using System.Globalization;

var options = new WorksheetOptions()
    .SetupColumn("UsdAmount", new()
    {
        LocaleFormat = LocaleNumberFormatStyles.Currency,
        Culture = new CultureInfo("en-US"),
    })
    .SetupColumn("EurAmount", new()
    {
        LocaleFormat = LocaleNumberFormatStyles.Currency,
        Culture = new CultureInfo("de-DE"),
    })
    .SetupColumn("BrlAmount", new()
    {
        LocaleFormat = LocaleNumberFormatStyles.Currency,
        Culture = new CultureInfo("pt-BR"),
    });

await ExcelWriter.ExportAsync(outputStream, amounts, sheetName: "Amounts", sheetOptions: options);
```

Date column formatted for a specific locale:

```csharp
using System.Globalization;

var options = new WorksheetOptions()
    .SetupColumn("OrderDate", new()
    {
        LocaleFormat = LocaleNumberFormatStyles.DateShort,
        Culture = new CultureInfo("en-GB"), // renders as dd/MM/yyyy
    })
    .SetupColumn("ShipDate", new()
    {
        LocaleFormat = LocaleNumberFormatStyles.DateTime,
        Culture = new CultureInfo("en-US"), // renders as M/d/yyyy h:mm AM/PM
    });

await ExcelWriter.ExportAsync(outputStream, orders, sheetName: "Orders", sheetOptions: options);
```

The available `LocaleNumberFormatStyles` values are: `DateShort`, `DateLong`, `YearMonth`, `MonthDay`, `ShortTime`, `LongTime`, `DateTime`, `DateTimeFull`, `Currency`, `Number`.

### S3-Friendly Exports

This library is a good fit for S3-oriented export workflows because:

- The workbook is written incrementally, so memory stays bounded even for large exports.
- The ZIP writer does not require seeking, which makes it compatible with streaming-style destinations.
- You can write to any `Stream`, so the export step can plug into whatever upload pipeline your application already uses.

One practical pattern is to stream the workbook into a [`Pipe` from `System.IO.Pipelines`](https://learn.microsoft.com/en-us/dotnet/api/system.io.pipelines.pipe), which lets one side write bytes while the other side reads them. In this example, `ExcelWriter` writes to the `PipeWriter`, and the upload code reads from the `PipeReader` to assemble and upload `5 MiB` S3 multipart parts:

```csharp
using Amazon.S3.Model;
using System.IO.Pipelines;
using jaytwo.DataExport.Excel;

public static async Task ExportPeopleToS3Async(IAsyncEnumerable<Person> people)
{
    // Cancellation tokens and detailed AWS request parameters are omitted for brevity.

    var pipe = new Pipe();
    const int partSize = 5 * 1024 * 1024; // S3 requires each non-final multipart upload part to be at least 5 MiB.

    var multipartUpload = await s3.InitiateMultipartUploadAsync(...); // Supply the bucket name and object key here.

    // Start both tasks before awaiting either one so the pipe can stream data from the workbook writer to the S3 uploader.
    var writeWorkbookTask = Task.Run(async () =>
    {
        await using var output = pipe.Writer.AsStream();

        try
        {
            await ExcelWriter.ExportAsync(output, people, sheetName: "People");
            await pipe.Writer.CompleteAsync();
        }
        catch (Exception ex)
        {
            await pipe.Writer.CompleteAsync(ex);
            throw;
        }
    });

    var uploadPartsTask = Task.Run(async () =>
    {
        var buffer = new byte[partSize];
        var bufferedBytes = 0;
        var partNumber = 1;
        var uploadedParts = new List<PartETag>();
        int bytesRead;
        await using var input = pipe.Reader.AsStream();

        while ((bytesRead = await input.ReadAsync(buffer, bufferedBytes, partSize - bufferedBytes)) > 0)
        {
            bufferedBytes += bytesRead;

            if (bufferedBytes == partSize)
            {
                uploadedParts.Add(await UploadBufferedPartAsync(...)); // Upload one full-sized non-final part.
                bufferedBytes = 0;
                partNumber++;
            }
        }

        if (bufferedBytes > 0)
        {
            uploadedParts.Add(await UploadBufferedPartAsync(...)); // Upload the final partial part, if any bytes remain.
        }

        await s3.CompleteMultipartUploadAsync(...); // Finalize the object using the uploaded part list.
    });

    try
    {
        await Task.WhenAll(writeWorkbookTask, uploadPartsTask);
    }
    catch
    {
        await s3.AbortMultipartUploadAsync(...); // Abort the in-progress upload so unfinished parts are discarded.
        throw;
    }
}
```

The important part is the shape of the solution:

- `ExcelWriter` writes forward-only into the pipe.
- The upload side fills a `5 MiB` buffer from that pipe.
- Each full buffer becomes one S3 multipart upload part.
- The remaining bytes become the final part.
- If either side fails, abort the multipart upload so S3 does not retain unfinished parts.

That keeps memory bounded without needing to know the final workbook size ahead of time.

## Notes

- `ExportAsync` is the simplest API for one-sheet exports
- Constructing `ExcelWriter` directly is the intended API for multi-sheet exports
- `ExcelWriter` implements both `IDisposable` and `IAsyncDisposable`, so make sure it is disposed properly
- Important workbook data is written during disposal at both the XML and ZIP levels
- All output is written directly to the provided stream, for example a `FileStream`
- No temporary files or seekable streams are required
- Output is compatible with OpenXML consumers including Excel, Google Sheets, and LibreOffice

---

Made with &hearts; by Jake. Licensed under the [MIT License](https://mit-license.org/)
