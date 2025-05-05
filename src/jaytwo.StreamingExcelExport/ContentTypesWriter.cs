using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport;

public class ContentTypesWriter : XmlDocumentWriter
{
    public ContentTypesWriter(XmlWriter writer)
        : base(writer)
    {
    }

    public static string Path { get; } = "[Content_Types].xml";

    public async Task WriteAsync()
    {
        await using (await CreateDocumentScopeAsync(standalone: true))
        await using (CreateElementScope("Types", "http://schemas.openxmlformats.org/package/2006/content-types"))
        {
            await WriteDefaultElementAsync(extension: "rels", contentType: "application/vnd.openxmlformats-package.relationships+xml");
            await WriteDefaultElementAsync(extension: "xml", contentType: "application/xml");

            await WriteOverrideElementAsync(partName: "/xl/workbook.xml", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
            await WriteOverrideElementAsync(partName: "/xl/worksheets/sheet1.xml", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
            await WriteOverrideElementAsync(partName: "/docProps/core.xml", contentType: "application/vnd.openxmlformats-package.core-properties+xml");
            await WriteOverrideElementAsync(partName: "/docProps/app.xml", contentType: "application/vnd.openxmlformats-officedocument.extended-properties+xml");
        }
    }

    private async Task WriteDefaultElementAsync(string extension, string contentType)
        => await WriteElementWithAttributes("Default", new() { { "Extension", extension }, { "ContentType", contentType } });

    private async Task WriteOverrideElementAsync(string partName, string contentType)
        => await WriteElementWithAttributes("Override", new() { { "PartName", partName }, { "ContentType", contentType } });
}
