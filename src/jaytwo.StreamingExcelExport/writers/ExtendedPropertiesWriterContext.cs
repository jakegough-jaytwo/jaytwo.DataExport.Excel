using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

public class ExtendedPropertiesWriterContext : IWriterContext
{
    public ExtendedPropertiesWriterContext(string application, string appVersion, string company, IList<string> sheetNames)
    {
        Application = application;
        AppVersion = appVersion;
        Company = company;
        SheetNames = sheetNames;
    }

    public string ZipPackagePath => "docProps/app.xml";

    public string Application { get; }

    public string AppVersion { get; }

    public string Company { get; }

    public IList<string> SheetNames { get; }

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new ExtendedPropertiesWriter(this, writer).WriteAsync();
}
