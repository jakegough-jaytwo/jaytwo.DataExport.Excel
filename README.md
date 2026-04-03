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
- Async-first API supporting `IDataReader`, `IAsyncEnumerable<T>`, and `IEnumerable<T>`
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

For object-based exports, public properties of `T` become columns. For tabular exports, you can also write directly from an `IDataReader`.

> Warning: Be careful not to confuse `IEnumerable<T>` with concrete collections like `List<T>` or arrays (`T[]`).
>
> While `List<T>` and arrays do implement `IEnumerable<T>`, they are also `ICollection<T>`, meaning the entire dataset is already fully loaded into memory before you pass it to the writer. In these cases, the streaming benefits of this library are diminished or lost.
>
> To take advantage of streaming for large datasets, prefer `IAsyncEnumerable<T>`, `IDataReader`, or a lazily-yielding `IEnumerable<T>` generated with `yield return`.

> Warning: While this package is designed to write directly to a stream, use caution when writing to an HTTP stream.
>
> If an exception occurs halfway through the export and you have already set the HTTP status code to `200 OK`, the client has no reliable way to detect that the response was incomplete or corrupted.

### One-Shot Export to a Stream

```csharp
await ExcelWriter.ExportAsync(outputStream, GetPeopleAsync(), sheetName: "People");
```

`ExportAsync` also supports `IEnumerable<T>` and `IDataReader`:

```csharp
await ExcelWriter.ExportAsync(outputStream, people, sheetName: "People");
await ExcelWriter.ExportAsync(outputStream, dataReader, sheetName: "People");
```

### One-Shot Export to a File

```csharp
await ExcelWriter.ExportAsync("people.xlsx", GetPeopleAsync(), sheetName: "People");
```

File-path overloads are also available for `IEnumerable<T>` and `IDataReader`.

### Adding Custom Metadata

```csharp
var radMetadata = new WorkbookMetadata
{
    Creator = "Cru Jones",
    CompanyName = "Rad Racing",
    ApplicationName = "BMXcel",
    ApplicationVersion = "1986.3.21",
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

```csharp
var options = new WorksheetOptions(applyZebraStripe: true, freezeHeaderRow: true, boldHeaderRow: true)
    .SetupColumn("Name", new() { Width = 30 })
    .SetupColumn("Age", new() { Width = 7, HorizontalAlignment = HorizontalAlignmentStyles.Center })
    .SetupColumn("BirthDate", new() { Width = 20 });

await ExcelWriter.ExportAsync(outputStream, peopleData, sheetName: "People", sheetOptions: options);
```

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
