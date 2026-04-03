using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.OpenXml;
using jaytwo.DataExport.Excel.Writers;
using jaytwo.DataExport.Excel.Zip;

namespace jaytwo.DataExport.Excel;

public class ExcelWriter : IDisposable, IAsyncDisposable
{
    private const string DefaultSheetName = "Sheet1";

    private IZipWriter _zip;
    private RelationshipIndex _relationships;
    private WorksheetIndex _sheetsIndex;
    private bool _initialized;

    /// <summary>
    /// Initializes a new <see cref="ExcelWriter"/> that writes a workbook to the specified output stream.
    /// </summary>
    /// <param name="outputStream">The destination stream that will receive the XLSX content.</param>
    /// <param name="metadata">Optional workbook metadata written into the package.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="outputStream"/> open when the writer is disposed; otherwise, <see langword="false"/>.</param>
    public ExcelWriter(
        Stream outputStream,
        WorkbookMetadata? metadata = default,
        bool leaveOpen = true)
    {
        _zip = ZipWriter.CreateZip32(outputStream, leaveOpen: leaveOpen);
        _relationships = new RelationshipIndex();
        _sheetsIndex = new WorksheetIndex(_relationships);
        WorkbookMetadata = metadata ?? new WorkbookMetadata();

        OutputStream = outputStream;
    }

    /// <summary>
    /// Gets the destination stream that receives the XLSX content.
    /// </summary>
    public Stream OutputStream { get; }

    /// <summary>
    /// Gets the workbook metadata written into the package.
    /// </summary>
    public WorkbookMetadata WorkbookMetadata { get; }

    /// <summary>
    /// Exports a single worksheet from an <see cref="IDataReader"/> to a new XLSX file.
    /// </summary>
    /// <param name="fileName">The path of the XLSX file to create.</param>
    /// <param name="data">The tabular data to write.</param>
    /// <param name="sheetName">The worksheet name to use in the workbook.</param>
    /// <param name="sheetOptions">Optional worksheet formatting and layout options.</param>
    /// <param name="metadata">Optional workbook metadata written into the package.</param>
    /// <param name="cancellationToken">A token used to cancel the export.</param>
    public static async Task ExportAsync(
        string fileName,
        IDataReader data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        WorkbookMetadata? metadata = default,
        CancellationToken cancellationToken = default)
    {
        using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        await ExportAsync(fileStream, data, sheetName, sheetOptions, metadata, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Exports a single worksheet from an <see cref="IDataReader"/> to an XLSX stream.
    /// </summary>
    /// <param name="outputStream">The destination stream that will receive the XLSX content.</param>
    /// <param name="data">The tabular data to write.</param>
    /// <param name="sheetName">The worksheet name to use in the workbook.</param>
    /// <param name="sheetOptions">Optional worksheet formatting and layout options.</param>
    /// <param name="metadata">Optional workbook metadata written into the package.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="outputStream"/> open after export; otherwise, <see langword="false"/>.</param>
    /// <param name="cancellationToken">A token used to cancel the export.</param>
    public static async Task ExportAsync(
        Stream outputStream,
        IDataReader data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        WorkbookMetadata? metadata = default,
        bool leaveOpen = true,
        CancellationToken cancellationToken = default)
    {
        using var writer = new ExcelWriter(outputStream, metadata, leaveOpen);
        await writer.WriteSheetAsync(data, sheetName, sheetOptions, cancellationToken);
    }

    /// <summary>
    /// Exports a single worksheet from an asynchronous sequence to a new XLSX file.
    /// </summary>
    /// <typeparam name="T">The row type whose public properties become worksheet columns.</typeparam>
    /// <param name="fileName">The path of the XLSX file to create.</param>
    /// <param name="data">The asynchronous sequence of rows to write.</param>
    /// <param name="sheetName">The worksheet name to use in the workbook.</param>
    /// <param name="sheetOptions">Optional worksheet formatting and layout options.</param>
    /// <param name="metadata">Optional workbook metadata written into the package.</param>
    /// <param name="cancellationToken">A token used to cancel the export.</param>
    public static async Task ExportAsync<T>(
        string fileName,
        IAsyncEnumerable<T> data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        WorkbookMetadata? metadata = default,
        CancellationToken cancellationToken = default)
    {
        using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        await ExportAsync(fileStream, data, sheetName, sheetOptions, metadata, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Exports a single worksheet from a synchronous sequence to a new XLSX file.
    /// </summary>
    /// <typeparam name="T">The row type whose public properties become worksheet columns.</typeparam>
    /// <param name="fileName">The path of the XLSX file to create.</param>
    /// <param name="data">The sequence of rows to write.</param>
    /// <param name="sheetName">The worksheet name to use in the workbook.</param>
    /// <param name="sheetOptions">Optional worksheet formatting and layout options.</param>
    /// <param name="metadata">Optional workbook metadata written into the package.</param>
    /// <param name="cancellationToken">A token used to cancel the export.</param>
    public static async Task ExportAsync<T>(
        string fileName,
        IEnumerable<T> data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        WorkbookMetadata? metadata = default,
        CancellationToken cancellationToken = default)
    {
        using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        await ExportAsync(fileStream, data, sheetName, sheetOptions, metadata, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Exports a single worksheet from an asynchronous sequence to an XLSX stream.
    /// </summary>
    /// <typeparam name="T">The row type whose public properties become worksheet columns.</typeparam>
    /// <param name="outputStream">The destination stream that will receive the XLSX content.</param>
    /// <param name="data">The asynchronous sequence of rows to write.</param>
    /// <param name="sheetName">The worksheet name to use in the workbook.</param>
    /// <param name="sheetOptions">Optional worksheet formatting and layout options.</param>
    /// <param name="metadata">Optional workbook metadata written into the package.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="outputStream"/> open after export; otherwise, <see langword="false"/>.</param>
    /// <param name="cancellationToken">A token used to cancel the export.</param>
    public static async Task ExportAsync<T>(
        Stream outputStream,
        IAsyncEnumerable<T> data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        WorkbookMetadata? metadata = default,
        bool leaveOpen = true,
        CancellationToken cancellationToken = default)
    {
        using var writer = new ExcelWriter(outputStream, metadata, leaveOpen);
        await writer.WriteSheetAsync(data, sheetName, sheetOptions, cancellationToken);
    }

    /// <summary>
    /// Exports a single worksheet from a synchronous sequence to an XLSX stream.
    /// </summary>
    /// <typeparam name="T">The row type whose public properties become worksheet columns.</typeparam>
    /// <param name="outputStream">The destination stream that will receive the XLSX content.</param>
    /// <param name="data">The sequence of rows to write.</param>
    /// <param name="sheetName">The worksheet name to use in the workbook.</param>
    /// <param name="sheetOptions">Optional worksheet formatting and layout options.</param>
    /// <param name="metadata">Optional workbook metadata written into the package.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="outputStream"/> open after export; otherwise, <see langword="false"/>.</param>
    /// <param name="cancellationToken">A token used to cancel the export.</param>
    public static async Task ExportAsync<T>(
        Stream outputStream,
        IEnumerable<T> data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        WorkbookMetadata? metadata = default,
        bool leaveOpen = true,
        CancellationToken cancellationToken = default)
    {
        using var writer = new ExcelWriter(outputStream, metadata, leaveOpen);
        await writer.WriteSheetAsync(data, sheetName, sheetOptions, cancellationToken);
    }

    /// <summary>
    /// Writes a worksheet from an <see cref="IDataReader"/> into the current workbook.
    /// </summary>
    /// <param name="data">The tabular data to write.</param>
    /// <param name="sheetName">The worksheet name to use in the workbook.</param>
    /// <param name="sheetOptions">Optional worksheet formatting and layout options.</param>
    /// <param name="cancellationToken">A token used to cancel the write operation.</param>
    public async Task WriteSheetAsync(
        IDataReader data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        CancellationToken cancellationToken = default)
    {
        await WriteStartAsync(cancellationToken);

        sheetOptions ??= new WorksheetOptions();
        var sheetSpec = _sheetsIndex.Add(sheetName);
        await WriteAsync(new WorksheetWriterDataReaderContext(sheetSpec.SheetTag, sheetSpec.WorksheetUid, sheetOptions, data), cancellationToken);
    }

    /// <summary>
    /// Writes a worksheet from a synchronous sequence into the current workbook.
    /// </summary>
    /// <typeparam name="T">The row type whose public properties become worksheet columns.</typeparam>
    /// <param name="data">The sequence of rows to write.</param>
    /// <param name="sheetName">The worksheet name to use in the workbook.</param>
    /// <param name="sheetOptions">Optional worksheet formatting and layout options.</param>
    /// <param name="cancellationToken">A token used to cancel the write operation.</param>
    public async Task WriteSheetAsync<T>(
        IEnumerable<T> data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        CancellationToken cancellationToken = default)
        => await WriteSheetAsync(ToAsyncEnumerable(data, cancellationToken), sheetName, sheetOptions, cancellationToken);

    /// <summary>
    /// Writes a worksheet from an asynchronous sequence into the current workbook.
    /// </summary>
    /// <typeparam name="T">The row type whose public properties become worksheet columns.</typeparam>
    /// <param name="data">The asynchronous sequence of rows to write.</param>
    /// <param name="sheetName">The worksheet name to use in the workbook.</param>
    /// <param name="sheetOptions">Optional worksheet formatting and layout options.</param>
    /// <param name="cancellationToken">A token used to cancel the write operation.</param>
    public async Task WriteSheetAsync<T>(
        IAsyncEnumerable<T> data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        CancellationToken cancellationToken = default)
    {
        await WriteStartAsync(cancellationToken);

        sheetOptions ??= new WorksheetOptions();
        var sheetSpec = _sheetsIndex.Add(sheetName);
        await WriteAsync(new WorksheetWriterContext<T>(sheetSpec.SheetTag, sheetSpec.WorksheetUid, sheetOptions, data), cancellationToken);
    }

    /// <summary>
    /// Writes the workbook stylesheet entry.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the write operation.</param>
    public async Task WriteStyleSheetAsync(CancellationToken cancellationToken = default)
    {
        _relationships.AddStyleSheet();
        await WriteAsync(new StylesWriterContext(), cancellationToken);
    }

    /// <summary>
    /// Asynchronously finishes the workbook and releases associated resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await WriteFinishAsync();
        await _zip.DisposeAsync();
    }

    /// <summary>
    /// Finishes the workbook and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        try
        {
            WriteFinishAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            // optionally log or rethrow
            throw new IOException("Error during synchronous disposal of ExcelWriter", ex);
        }

        _zip.Dispose();
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
            await Task.Yield(); // ensures it's really async
        }
    }

    private async Task WriteStartAsync(CancellationToken cancellationToken)
    {
        if (!_initialized)
        {
            await WriteAsync(new DotRelsWriterContext(), cancellationToken);
            await WriteAsync(BuildCorePropertiesWriterContext(), cancellationToken);
            await WriteStyleSheetAsync(cancellationToken);
            _initialized = true;
        }
    }

    private async ValueTask WriteFinishAsync(CancellationToken cancellationToken = default)
    {
        await WriteAsync(new WorkbookRelationshipsWriterContext(_relationships.Relationships), cancellationToken);
        await WriteAsync(new WorkbookWriterContext(_sheetsIndex.Sheets), cancellationToken);
        await WriteAsync(new ContentTypesWriterContext(_sheetsIndex.SheetTags, _relationships.HasStyleSheet), cancellationToken);
        await WriteAsync(BuildAppPropertiesWriterContext(), cancellationToken); // needs to be after sheets are written
        await OutputStream.FlushAsync(cancellationToken);
    }

    private async Task WriteAsync(IWriterContext context, CancellationToken cancellationToken)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = false,
            Async = true,
            CloseOutput = false,
        };

        await using (var entryStream = _zip.OpenEntryStream(context.ZipPackagePath))
        using (var writer = XmlWriter.Create(entryStream, settings))
        {
            await context.WriteAsync(writer, cancellationToken);
        }
    }

    private ExtendedPropertiesWriterContext BuildAppPropertiesWriterContext()
        => new(
            application: WorkbookMetadata.ApplicationName,
            appVersion: WorkbookMetadata.ApplicationVersion,
            company: WorkbookMetadata.CompanyName,
            sheetNames: _sheetsIndex.SheetNames);

    private CorePropertiesWriterContext BuildCorePropertiesWriterContext()
        => new(
            creator: WorkbookMetadata.Creator,
            lastModifiedBy: WorkbookMetadata.LastModifiedBy ?? WorkbookMetadata.Creator,
            createdUtc: WorkbookMetadata.CreatedAtUtc,
            modifiedUtc: WorkbookMetadata.ModifiedAtUtc,
            title: WorkbookMetadata.Title,
            subject: WorkbookMetadata.Subject,
            description: WorkbookMetadata.Description,
            keywords: WorkbookMetadata.Keywords,
            category: WorkbookMetadata.Category);
}
