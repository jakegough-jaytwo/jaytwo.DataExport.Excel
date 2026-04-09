using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.OpenXml;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class WorkbookWriter : XmlDocumentWriter
{
    private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private readonly WorksheetSpec[] _sheets;

    public WorkbookWriter(WorksheetSpec[] sheets)
    {
        _sheets = sheets;
    }

    public override string ZipPackagePath => "xl/workbook.xml";

    protected override async Task WriteRootElementAsync(XmlWriter writer, CancellationToken cancellationToken)
    {
        await using (CreateElementScope(writer, "workbook", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"))
        {
            WriteAttributeString(writer, "xmlns", "r", null, RelationshipsNamespace);
            WriteAttributeString(writer, "xmlns", "mc", null, "http://schemas.openxmlformats.org/markup-compatibility/2006");
            WriteAttributeString(writer, "mc", "Ignorable", "http://schemas.openxmlformats.org/markup-compatibility/2006", "x15 xr xr6 xr10 xr2");
            WriteAttributeString(writer, "xmlns", "x15", null, "http://schemas.microsoft.com/office/spreadsheetml/2010/11/main");
            WriteAttributeString(writer, "xmlns", "xr", null, "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");
            WriteAttributeString(writer, "xmlns", "xr6", null, "http://schemas.microsoft.com/office/spreadsheetml/2016/revision6");
            WriteAttributeString(writer, "xmlns", "xr10", null, "http://schemas.microsoft.com/office/spreadsheetml/2016/revision10");
            WriteAttributeString(writer, "xmlns", "xr2", null, "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");

            await using (CreateElementScope(writer, "sheets"))
            {
                foreach (var sheet in _sheets)
                {
                    await WriteSheetElementAsync(writer, name: sheet.SheetName, sheetId: sheet.SheetId, rid: sheet.RelationshipId);
                }
            }
        }
    }

    private async Task WriteSheetElementAsync(XmlWriter writer, string name, string sheetId, string rid)
    {
        await using (CreateElementScopeWithAttributes(writer, "sheet", new() { { "name", name }, { "sheetId", sheetId } }))
        {
            WriteAttributeString(writer, "r", "id", RelationshipsNamespace, rid);
        }
    }
}
