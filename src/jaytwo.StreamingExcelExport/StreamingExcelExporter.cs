using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.StreamingExcelExport.OpenXml;
using jaytwo.StreamingExcelExport.Writers;
using jaytwo.StreamingExcelExport.Zip;

namespace jaytwo.StreamingExcelExport;

public class StreamingExcelExporter : IDisposable, IAsyncDisposable
{
    private const string DefaultSheetName = "Sheet1";

    private IZipWriter _zip;
    private RelationshipIndex _relationships;
    private WorksheetIndex _sheetsIndex;

    public StreamingExcelExporter(
        Stream outputStream,
        string applicationName = "MyApplication",
        string applicationVersion = "1.0",
        string companyName = "My Company",
        string createdBy = "Me",
        DateTime? createdAtUtc = default,
        bool useZip64 = true,
        bool leaveInnerStreamOpen = true)
    {
        _zip = ZipWriter.Create(outputStream, leaveOpen: leaveInnerStreamOpen, useZip64: useZip64);
        _relationships = new RelationshipIndex();
        _sheetsIndex = new WorksheetIndex(_relationships);

        OutputStream = outputStream;
        ApplicationName = applicationName;
        ApplicationVersion = applicationVersion;
        CompanyName = companyName;
        Creator = createdBy;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public Stream OutputStream { get; }

    public string ApplicationName { get; }

    public string ApplicationVersion { get; }

    public string CompanyName { get; }

    public string Creator { get; }

    public DateTime CreatedAtUtc { get; }

    public async Task WriteSheetAsync<T>(
        IEnumerable<T> data,
        string sheetName = DefaultSheetName,
        CancellationToken cancellationToken = default)
        => await WriteSheetAsync(ToAsyncEnumerable(data, cancellationToken), sheetName, cancellationToken);

    public async Task WriteSheetAsync<T>(
        IAsyncEnumerable<T> data,
        string sheetName = DefaultSheetName,
        CancellationToken cancellationToken = default)
    {
        var sheetSpec = _sheetsIndex.Add(sheetName);
        await WriteAsync(new WorksheetWriterContext<T>(sheetSpec.SheetTag, data), cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await WriteFinishAsync();
        await _zip.DisposeAsync();
    }

    public void Dispose()
    {
        WriteFinishAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
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

    private async ValueTask WriteFinishAsync(CancellationToken cancellationToken = default)
    {
        await WriteAsync(new DotRelsWriterContext(), cancellationToken);
        await WriteAsync(new CorePropertiesWriterContext(Creator, CreatedAtUtc), cancellationToken);
        await WriteAsync(new WorkbookRelationshipsWriterContext(_relationships.Relationshnips), cancellationToken);
        await WriteAsync(new WorkbookWriterContext(_sheetsIndex.Sheets), cancellationToken);
        await WriteAsync(new ContentTypesWriterContext(_sheetsIndex.SheetTags, _relationships.HasStyleSheet), cancellationToken);
        await WriteAsync(BuildAppPropertiesWriterContext(), cancellationToken);
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

        await _zip.WriteFileAsync(
            fileName: context.ZipPackagePath,
            comment: string.Empty,
            writeFileCallback: async stream =>
            {
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    await context.WriteAsync(writer, cancellationToken);
                }
            });
    }

    private ExtendedPropertiesWriterContext BuildAppPropertiesWriterContext()
        => new(
            application: ApplicationName,
            appVersion: ApplicationVersion,
            company: CompanyName,
            sheetNames: _sheetsIndex.SheetNames);
}
