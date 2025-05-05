using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport;

public class WorkbookRelsWriter : RelsWriter
{
    public WorkbookRelsWriter(XmlWriter writer)
        : base(writer)
    {
    }

    public static string Path { get; } = "xl/_rels/workbook.xml.rels";

    protected override async Task WriteRelationshipELementsAsync()
    {
        await WriteRelationshipElementAsync(
            id: "rId1",
            type: "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet",
            target: "worksheets/sheet1.xml");
    }
}
