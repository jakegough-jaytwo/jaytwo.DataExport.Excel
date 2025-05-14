using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class ContentTypesWriter : XmlDocumentWriter
{
    public ContentTypesWriter(ContentTypesWriterContext context, XmlWriter writer)
        : base(writer)
    {
        Context = context;
    }

    public ContentTypesWriterContext Context { get; }

    protected override async Task WriteRootElementAsync(CancellationToken cancellationToken)
    {
        await using (CreateElementScope("Types", "http://schemas.openxmlformats.org/package/2006/content-types"))
        {
            await WriteDefaultElementAsync(extension: "rels", contentType: "application/vnd.openxmlformats-package.relationships+xml");
            await WriteDefaultElementAsync(extension: "xml", contentType: "application/xml");

            await WriteOverrideElementAsync(partName: "/xl/workbook.xml", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");

            foreach (var sheetTag in Context.SheetTags)
            {
                await WriteOverrideElementAsync(partName: $"/xl/worksheets/{sheetTag}.xml", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
            }

            if (Context.HasStyleSheet)
            {
                await WriteOverrideElementAsync(partName: "/xl/styles.xml", contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml");
            }

            await WriteOverrideElementAsync(partName: "/docProps/core.xml", contentType: "application/vnd.openxmlformats-package.core-properties+xml");
            await WriteOverrideElementAsync(partName: "/docProps/app.xml", contentType: "application/vnd.openxmlformats-officedocument.extended-properties+xml");
        }
    }

    private async Task WriteDefaultElementAsync(string extension, string contentType)
        => await WriteElementWithAttributes("Default", new() { { "Extension", extension }, { "ContentType", contentType } });

    private async Task WriteOverrideElementAsync(string partName, string contentType)
        => await WriteElementWithAttributes("Override", new() { { "PartName", partName }, { "ContentType", contentType } });
}
