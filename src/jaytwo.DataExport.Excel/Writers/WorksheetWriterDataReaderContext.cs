using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class WorksheetWriterDataReaderContext : IWriterContext
{
    private readonly string _sheetTag;
    private readonly string _worksheetUid;
    private readonly WorksheetOptions _options;
    private readonly IDataReader _data;
    private readonly StyleRegistry _styleRegistry;

    public WorksheetWriterDataReaderContext(string sheetTag, string worksheetUid, WorksheetOptions options, IDataReader data, StyleRegistry styleRegistry)
    {
        _sheetTag = sheetTag;
        _worksheetUid = worksheetUid;
        _options = options;
        _data = data;
        _styleRegistry = styleRegistry;
    }

    public string ZipPackagePath => $"xl/worksheets/{_sheetTag}.xml";

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new WorksheetWriterDataReader(_worksheetUid, _options, _data, writer, _styleRegistry).WriteAsync(cancellationToken);
}
