using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

internal class WorksheetWriterContext<T> : IWriterContext
{
    public WorksheetWriterContext(string sheetTag, IAsyncEnumerable<T> data)
    {
        SheetTag = sheetTag;
        Data = data;
    }

    public string ZipPackagePath => $"xl/worksheets/{SheetTag}.xml";

    public string SheetTag { get; set; }

    public bool IncludeHeader { get; set; } = true;

    public IAsyncEnumerable<T> Data { get; set; }

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new WorksheetWriter<T>(this, writer).WriteAsync(cancellationToken);
}
