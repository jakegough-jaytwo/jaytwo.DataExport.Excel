using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class WorksheetWriterContext<T> : IWriterContext
{
    public WorksheetWriterContext(string sheetTag, WorksheetOptions options, IAsyncEnumerable<T> data)
    {
        SheetTag = sheetTag;
        Data = data;
        Options = options;
    }

    public string ZipPackagePath => $"xl/worksheets/{SheetTag}.xml";

    public string SheetTag { get; set; }

    public WorksheetOptions Options { get; set; }

    public IAsyncEnumerable<T> Data { get; set; }

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new WorksheetWriter<T>(this, writer).WriteAsync(cancellationToken);
}
