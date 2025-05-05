using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DisappearingFiles;

namespace jaytwo.StreamingExcelExport;

public class StreamingExcelExporter<T>
{
    private const string DefaultSheetName = "Sheet1";

    private readonly IList<PropertyInfo> _props;

    public StreamingExcelExporter()
    {
        _props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    public async ValueTask WriteDataAsync(
        string filePath,
        IAsyncEnumerable<T> data,
        string sheetName = DefaultSheetName,
        CancellationToken cancellationToken = default)
    {
        using var outputStream = File.OpenWrite(filePath);
        await WriteDataAsync(outputStream, data, sheetName, cancellationToken);
    }

    public async ValueTask WriteDataAsync(
        Stream outputStream,
        IAsyncEnumerable<T> data,
        string sheetName = DefaultSheetName,
        CancellationToken cancellationToken = default)
    {
        using var workspace = DisappearingDirectory.CreateInTempPath();
        var subfolder = workspace.CreateNewSubdirectory("xlsx");

        var dotRels = CreateFile(subfolder, DotRelsWriter.Path);
        using (var stream = dotRels.Create())
        using (var writer = CreateXmlWriter(stream))
        {
            await new DotRelsWriter(writer).WriteAsync();
        }

        var docProps = CreateFile(subfolder, AppPropertiesWriter.Path);
        using (var stream = docProps.Create())
        using (var writer = CreateXmlWriter(stream))
        {
            await new AppPropertiesWriter(writer).WriteAsync("MyApplication", "0.1", "My Company");
        }

        var coreProps = CreateFile(subfolder, CorePropertiesWriter.Path);
        using (var stream = coreProps.Create())
        using (var writer = CreateXmlWriter(stream))
        {
            await new CorePropertiesWriter(writer).WriteAsync("John Doe");
        }

        var workbookRels = CreateFile(subfolder, WorkbookRelsWriter.Path);
        using (var stream = workbookRels.Create())
        using (var writer = CreateXmlWriter(stream))
        {
            await new WorkbookRelsWriter(writer).WriteAsync();
        }

        var workbook = CreateFile(subfolder, WorkbookWriter.Path);
        using (var stream = workbook.Create())
        using (var writer = CreateXmlWriter(stream))
        {
            await new WorkbookWriter(writer).WriteAsync(sheetName);
        }

        var contentTypes = CreateFile(subfolder, ContentTypesWriter.Path);
        using (var stream = contentTypes.Create())
        using (var writer = CreateXmlWriter(stream))
        {
            await new ContentTypesWriter(writer).WriteAsync();
        }

        var worksheet = CreateFile(subfolder, WorksheetWriter<T>.Path);
        using (var stream = worksheet.Create())
        using (var writer = CreateXmlWriter(stream))
        {
            await new WorksheetWriter<T>(writer).WriteAsync(data, cancellationToken);
        }

        var xlsxFile = workspace.GetFullPath("foo.xlsx");
        ZipHelper.CreateZipFromFolder(subfolder.FullName, xlsxFile);
        using (var file = File.OpenRead(xlsxFile))
        {
            await file.CopyToAsync(outputStream);
        }
    }

    public FileInfo CreateFile(DirectoryInfo subfolder, string relativePath)
    {
        var parts = relativePath.Replace("\\", "/").Split("/");
        var folders = parts.Take(parts.Length - 1);
        var file = parts.Last();

        var progressivePath = string.Empty;
        foreach (var folder in folders)
        {
            progressivePath = Path.Combine(progressivePath, folder);
            var fullPath = Path.Combine(subfolder.FullName, progressivePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        return new FileInfo(Path.Combine(subfolder.FullName, relativePath));
    }

    public async Task WriteDataAsync(
        string filePath,
        IEnumerable<T> data,
        string sheetName = DefaultSheetName,
        CancellationToken cancellationToken = default)
        => await WriteDataAsync(filePath, ToAsyncEnumerable(data), sheetName);

    public async Task WriteDataAsync(
        Stream stream,
        IEnumerable<T> data,
        string sheetName = DefaultSheetName,
        CancellationToken cancellationToken = default)
        => await WriteDataAsync(stream, ToAsyncEnumerable(data), sheetName);

    private static XmlWriter CreateXmlWriter(Stream outputStream)
    {
        var settings = new XmlWriterSettings
        {
            //Indent = true,
            Indent = false,
            Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = false,
            Async = true,
        };

        return XmlWriter.Create(outputStream, settings);
    }

    private static async IAsyncEnumerable<TElement> ToAsyncEnumerable<TElement>(
        IEnumerable<TElement> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
            await Task.Yield(); // ensures it's really async
        }
    }
}
