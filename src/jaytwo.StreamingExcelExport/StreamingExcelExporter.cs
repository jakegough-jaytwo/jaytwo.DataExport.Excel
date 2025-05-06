using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.StreamingExcelExport.Writers;

namespace jaytwo.StreamingExcelExport;

public class StreamingExcelExporter : IDisposable, IAsyncDisposable
{
    private const string DefaultSheetName = "Sheet1";

    private ZipWriter _zip;
    private RelationshipIndex _relationships;
    private WorksheetIndex _sheetsIndex;

    public StreamingExcelExporter(
        Stream outputStream,
        string applicationName = "MyApplication",
        string applicationVersion = "1.0",
        string companyName = "My Company",
        string createdBy = "Me",
        DateTime? createdAtUtc = default,
        bool leaveInnerStreamOpen = true)
    {
        _zip = new ZipWriter(outputStream, leaveOpen: leaveInnerStreamOpen);
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
        await WriteAsync(BuildCorePropertiesWriterContext(), cancellationToken);
        await WriteAsync(BuildWorkbookRelsWriterContext(), cancellationToken);
        await WriteAsync(BuildWorkbookWriterContext(), cancellationToken);
        await WriteAsync(BuildContentTypesWriterContext(), cancellationToken);
        await WriteAsync(BuildAppPropertiesWriterContext(), cancellationToken);
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

    private WorkbookWriterContext BuildWorkbookWriterContext()
        => new(_sheetsIndex.Sheets);

    private ContentTypesWriterContext BuildContentTypesWriterContext()
        => new(_sheetsIndex.SheetTags);

    private WorkbookRelationshipsWriterContext BuildWorkbookRelsWriterContext()
        => new(_relationships.Relationshnips);

    private CorePropertiesWriterContext BuildCorePropertiesWriterContext()
        => new(Creator, CreatedAtUtc);
}
