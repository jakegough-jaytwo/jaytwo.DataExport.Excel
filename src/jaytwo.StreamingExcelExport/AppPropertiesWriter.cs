using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport;

public class AppPropertiesWriter : XmlDocumentWriter
{
    private const string VTNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";

    public AppPropertiesWriter(XmlWriter writer)
        : base(writer)
    {
    }

    public static string Path { get; } = "docProps/app.xml";

    public async Task WriteAsync(string application, string appVersion, string company)
    {
        await using (await CreateDocumentScopeAsync(standalone: true))
        await using (CreateElementScope("Properties", "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties"))
        {
            WriteAttributeString("xmlns", "vt", null, VTNamespace);

            WriteElementString("Application", application);
            WriteElementString("DocSecurity", "0");
            WriteElementString("ScaleCrop", "false");

            await using (CreateElementScope("HeadingPairs"))
            {
                await using (CreateElementScope("vt", "vector", VTNamespace))
                {
                    WriteAttributeString("size", "2");
                    WriteAttributeString("baseType", "variant");

                    await using (CreateElementScope("vt", "variant", VTNamespace))
                    {
                        await WriteElementStringAsync("vt", "lpstr", VTNamespace, "Worksheets");
                    }

                    await using (CreateElementScope("vt", "variant", VTNamespace))
                    {
                        await WriteElementStringAsync("vt", "i4", VTNamespace, "1");
                    }
                }
            }

            await using (CreateElementScope("TitlesOfParts"))
            {
                await using (CreateElementScope("vt", "vector", VTNamespace))
                {
                    WriteAttributeString("size", "1");
                    WriteAttributeString("baseType", "lpstr");

                    await WriteElementStringAsync("vt", "lpstr", VTNamespace, "Sheet1");
                }
            }

            WriteElementString("Company", company);
            WriteElementString("LinksUpToDate", "false");
            WriteElementString("SharedDoc", "false");
            WriteElementString("HyperlinksChanged", "false");
            WriteElementString("AppVersion", appVersion);
        }
    }
}
