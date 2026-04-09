using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers.Xml;

internal abstract class XmlDocumentWriter
{
    public abstract string ZipPackagePath { get; }

    public virtual async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        await using (await CreateDocumentElementScope(writer))
        {
            await WriteRootElementAsync(writer, cancellationToken);
        }

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        await writer.FlushAsync();
    }

    protected virtual async Task<XmlDocumentScope> CreateDocumentElementScope(XmlWriter writer)
        => await CreateDocumentScopeAsync(writer, standalone: true);

    protected abstract Task WriteRootElementAsync(XmlWriter writer, CancellationToken cancellationToken);

    protected async Task WriteElementWithAttributes(XmlWriter writer, string elementName, Dictionary<string, string> attributes)
    {
        await using (CreateElementScope(writer, elementName))
        {
            foreach (var attribute in attributes)
            {
                WriteAttributeString(writer, attribute.Key, attribute.Value);
            }
        }
    }

    protected async Task<XmlDocumentScope> CreateDocumentScopeAsync(XmlWriter writer, bool standalone)
        => await XmlDocumentScope.CreateAsync(writer, standalone);

    protected XmlElementScope CreateElementScope(XmlWriter writer, string elementName)
        => XmlElementScope.Create(writer, elementName);

    protected XmlElementScope CreateElementScopeWithAttributes(XmlWriter writer, string elementName, Dictionary<string, string> attributes)
    {
        var scope = CreateElementScope(writer, elementName);

        foreach (var attribute in attributes)
        {
            WriteAttributeString(writer, attribute.Key, attribute.Value);
        }

        return scope;
    }

    protected XmlElementScope CreateElementScope(XmlWriter writer, string elementName, string ns)
        => XmlElementScope.Create(writer, elementName, ns);

    protected XmlElementScope CreateElementScope(XmlWriter writer, string prefix, string elementName, string ns)
        => XmlElementScope.Create(writer, prefix, elementName, ns);

    protected void WriteAttributeString(XmlWriter writer, string localName, string value)
        => writer.WriteAttributeString(localName, value);

    protected void WriteAttributeString(XmlWriter writer, string localName, string ns, string value)
        => writer.WriteAttributeString(localName, ns, value);

    protected void WriteAttributeString(XmlWriter writer, string prefix, string localName, string? ns, string value)
        => writer.WriteAttributeString(prefix, localName, ns, value);

    protected void WriteElementString(XmlWriter writer, string localName, string? value)
        => writer.WriteElementString(localName, value);

    protected void WriteElementString(XmlWriter writer, string localName, string? ns, string? value)
        => writer.WriteElementString(localName, ns, value);

    protected void WriteElementString(XmlWriter writer, string? prefix, string localName, string? ns, string? value)
        => writer.WriteElementString(prefix, localName, ns, value);

    protected async Task WriteElementStringAsync(XmlWriter writer, string? prefix, string localName, string? ns, string value)
        => await writer.WriteElementStringAsync(prefix, localName, ns, value);

    protected async Task WriteStringAsync(XmlWriter writer, string? text)
        => await writer.WriteStringAsync(text);
}
