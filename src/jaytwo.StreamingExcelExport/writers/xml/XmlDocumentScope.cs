using System;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers.Xml;

public class XmlDocumentScope : IDisposable, IAsyncDisposable
{
    private readonly XmlWriter _writer;

    private XmlDocumentScope(XmlWriter writer)
    {
        _writer = writer;
    }

    public static async Task<XmlDocumentScope> CreateAsync(XmlWriter writer, bool standalone)
    {
        await writer.WriteStartDocumentAsync(standalone: standalone);
        return new XmlDocumentScope(writer);
    }

    public async ValueTask DisposeAsync() => await _writer.WriteEndDocumentAsync();

    public void Dispose() => _writer.WriteEndDocument();
}
