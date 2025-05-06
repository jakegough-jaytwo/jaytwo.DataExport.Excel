using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

public class WorkbookRelationshipsWriterContext : IWriterContext
{
    public WorkbookRelationshipsWriterContext(IList<RelationshipSpec> relationships)
    {
        Relationships = relationships;
    }

    public string ZipPackagePath => "xl/_rels/workbook.xml.rels";

    public IList<RelationshipSpec> Relationships { get; set; }

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new WorkbookRelationshipsWriter(this, writer).WriteAsync();
}
