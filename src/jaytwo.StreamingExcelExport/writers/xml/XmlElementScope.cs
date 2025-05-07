using System;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers.Xml;

internal class XmlElementScope : IDisposable, IAsyncDisposable
{
    private readonly XmlWriter _writer;

    private XmlElementScope(XmlWriter writer)
    {
        _writer = writer;
    }

    public static XmlElementScope Create(XmlWriter writer, string elementName)
    {
        writer.WriteStartElement(elementName);
        return new XmlElementScope(writer);
    }

    public static XmlElementScope Create(XmlWriter writer, string elementName, string ns)
    {
        writer.WriteStartElement(elementName, ns);
        return new XmlElementScope(writer);
    }

    public static XmlElementScope Create(XmlWriter writer, string prefix, string elementName, string ns)
    {
        writer.WriteStartElement(prefix, elementName, ns);
        return new XmlElementScope(writer);
    }

    public async ValueTask DisposeAsync() => await _writer.WriteEndElementAsync();

    public void Dispose() => _writer.WriteEndElement();
}
