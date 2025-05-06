using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

public class WorkbookWriter : XmlDocumentWriter
{
    private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    public WorkbookWriter(WorkbookWriterContext context, XmlWriter writer)
        : base(writer)
    {
        Context = context;
    }

    public WorkbookWriterContext Context { get; }

    protected override async Task WriteRootElementAsync()
    {
        await using (CreateElementScope("workbook", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"))
        {
            WriteAttributeString("xmlns", "r", null, RelationshipsNamespace);
            WriteAttributeString("xmlns", "mc", null, "http://schemas.openxmlformats.org/markup-compatibility/2006");
            WriteAttributeString("mc", "Ignorable", "http://schemas.openxmlformats.org/markup-compatibility/2006", "x15 xr xr6 xr10 xr2");
            WriteAttributeString("xmlns", "x15", null, "http://schemas.microsoft.com/office/spreadsheetml/2010/11/main");
            WriteAttributeString("xmlns", "xr", null, "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");
            WriteAttributeString("xmlns", "xr6", null, "http://schemas.microsoft.com/office/spreadsheetml/2016/revision6");
            WriteAttributeString("xmlns", "xr10", null, "http://schemas.microsoft.com/office/spreadsheetml/2016/revision10");
            WriteAttributeString("xmlns", "xr2", null, "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");

            await using (CreateElementScope("sheets"))
            {
                foreach (var sheet in Context.Sheets)
                {
                    await WriteSheetElementAsync(name: sheet.SheetName, sheetId: sheet.SheetId, rid: sheet.RelationshipId);
                }
            }
        }
    }

    private async Task WriteSheetElementAsync(string name, string sheetId, string rid)
    {
        await using (CreateElementScope("sheet"))
        {
            WriteAttributeString("name", name);
            WriteAttributeString("sheetId", sheetId);
            WriteAttributeString("r", "id", RelationshipsNamespace, rid);
        }
    }
}
