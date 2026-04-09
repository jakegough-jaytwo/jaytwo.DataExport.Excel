using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class ContentTypesWriter : XmlDocumentWriter
{
    private readonly string[] _sheetTags;
    private readonly bool _hasStyleSheet;

    public ContentTypesWriter(string[] sheetTags, bool hasStyleSheet)
    {
        _sheetTags = sheetTags;
        _hasStyleSheet = hasStyleSheet;
    }

    public override string ZipPackagePath => "[Content_Types].xml";

    protected override async Task WriteRootElementAsync(XmlWriter writer, CancellationToken cancellationToken)
    {
        await using (CreateElementScope(writer, "Types", "http://schemas.openxmlformats.org/package/2006/content-types"))
        {
            await WriteDefaultElementAsync(writer, extension: "rels", contentType: "application/vnd.openxmlformats-package.relationships+xml");
            await WriteDefaultElementAsync(writer, extension: "xml", contentType: "application/xml");

            await WriteOverrideElementAsync(writer, partName: "/xl/workbook.xml", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");

            foreach (var sheetTag in _sheetTags)
            {
                await WriteOverrideElementAsync(writer, partName: $"/xl/worksheets/{sheetTag}.xml", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
            }

            if (_hasStyleSheet)
            {
                await WriteOverrideElementAsync(writer, partName: "/xl/styles.xml", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml");
            }

            await WriteOverrideElementAsync(writer, partName: "/docProps/core.xml", contentType: "application/vnd.openxmlformats-package.core-properties+xml");
            await WriteOverrideElementAsync(writer, partName: "/docProps/app.xml", contentType: "application/vnd.openxmlformats-officedocument.extended-properties+xml");
        }
    }

    private async Task WriteDefaultElementAsync(XmlWriter writer, string extension, string contentType)
        => await WriteElementWithAttributes(writer, "Default", new() { { "Extension", extension }, { "ContentType", contentType } });

    private async Task WriteOverrideElementAsync(XmlWriter writer, string partName, string contentType)
        => await WriteElementWithAttributes(writer, "Override", new() { { "PartName", partName }, { "ContentType", contentType } });
}
