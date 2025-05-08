using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers.Xml;

internal abstract class XmlDocumentWriter
{
    public XmlDocumentWriter(XmlWriter writer)
    {
        Writer = writer;
    }

    protected XmlWriter Writer { get; }

    public async Task WriteAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        await using (await CreateDocumentElementScope())
        {
            await WriteRootElementAsync(cancellationToken);
        }

        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        await Writer.FlushAsync();
    }

    protected virtual async Task<XmlDocumentScope> CreateDocumentElementScope()
        => await CreateDocumentScopeAsync(standalone: true);

    protected abstract Task WriteRootElementAsync(CancellationToken cancellationToken);

    protected async Task WriteElementWithAttributes(string elementName, Dictionary<string, string> attributes)
    {
        await using (CreateElementScope(elementName))
        {
            foreach (var attribute in attributes)
            {
                WriteAttributeString(attribute.Key, attribute.Value);
            }
        }
    }

    protected async Task<XmlDocumentScope> CreateDocumentScopeAsync(bool standalone)
        => await XmlDocumentScope.CreateAsync(Writer, standalone);

    protected XmlElementScope CreateElementScope(string elementName)
        => XmlElementScope.Create(Writer, elementName);

    protected XmlElementScope CreateElementScopeWithAttributes(string elementName, Dictionary<string, string> attributes)
    {
        var scope = CreateElementScope(elementName);

        foreach (var attribute in attributes)
        {
            WriteAttributeString(attribute.Key, attribute.Value);
        }

        return scope;
    }

    protected XmlElementScope CreateElementScope(string elementName, string ns)
        => XmlElementScope.Create(Writer, elementName, ns);

    protected XmlElementScope CreateElementScope(string prefix, string elementName, string ns)
        => XmlElementScope.Create(Writer, prefix, elementName, ns);

    protected void WriteAttributeString(string localName, string value)
        => Writer.WriteAttributeString(localName, value);

    protected void WriteAttributeString(string localName, string ns, string value)
        => Writer.WriteAttributeString(localName, ns, value);

    protected void WriteAttributeString(string prefix, string localName, string? ns, string value)
        => Writer.WriteAttributeString(prefix, localName, ns, value);

    protected void WriteElementString(string localName, string? value)
        => Writer.WriteElementString(localName, value);

    protected void WriteElementString(string localName, string? ns, string? value)
        => Writer.WriteElementString(localName, ns, value);

    protected void WriteElementString(string? prefix, string localName, string? ns, string? value)
        => Writer.WriteElementString(prefix, localName, ns, value);

    protected async Task WriteElementStringAsync(string? prefix, string localName, string? ns, string value)
        => await Writer.WriteElementStringAsync(prefix, localName, ns, value);

    protected async Task WriteStringAsync(string? text)
        => await Writer.WriteStringAsync(text);
}
