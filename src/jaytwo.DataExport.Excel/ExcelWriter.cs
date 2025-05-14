using System;
using System.Collections.Generic;
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

    public Stream OutputStream { get; }

    public WorkbookMetadata WorkbookMetadata { get; }

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

    public async Task WriteSheetAsync<T>(
        IEnumerable<T> data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        CancellationToken cancellationToken = default)
        => await WriteSheetAsync(ToAsyncEnumerable(data, cancellationToken), sheetName, sheetOptions, cancellationToken);

    public async Task WriteSheetAsync<T>(
        IAsyncEnumerable<T> data,
        string sheetName = DefaultSheetName,
        WorksheetOptions? sheetOptions = null,
        CancellationToken cancellationToken = default)
    {
        await WriteStartAsync(cancellationToken);

        sheetOptions ??= new WorksheetOptions();
        var sheetSpec = _sheetsIndex.Add(sheetName);
        await WriteAsync(new WorksheetWriterContext<T>(sheetSpec.SheetTag, sheetOptions, data), cancellationToken);
    }

    public async Task WriteStyleSheetAsync(CancellationToken cancellationToken = default)
    {
        _relationships.AddStyleSheet();
        await WriteAsync(new StylesWriterContext(), cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await WriteFinishAsync();
        await _zip.DisposeAsync();
    }

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
            await WriteAsync(new CorePropertiesWriterContext(WorkbookMetadata.Creator, WorkbookMetadata.CreatedAtUtc), cancellationToken);
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
}
