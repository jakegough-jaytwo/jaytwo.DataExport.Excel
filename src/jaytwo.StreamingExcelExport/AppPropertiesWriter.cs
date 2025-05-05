using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport;

public class AppPropertiesWriter : XmlDocumentWriter
{
    private const string VTNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";

    public AppPropertiesWriter(XmlWriter writer, string application, string appVersion, string company)
        : base(writer)
    {
        Application = application;
        AppVersion = appVersion;
        Company = company;
    }

    public static string PackagePath { get; } = "docProps/app.xml";

    public string Application { get; }

    public string AppVersion { get; }

    public string Company { get; }

    protected override async Task WriteRootElementAsync()
    {
        await using (CreateElementScope("Properties", "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties"))
        {
            WriteAttributeString("xmlns", "vt", null, VTNamespace);

            WriteElementString("Application", Application);
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

            WriteElementString("Company", Company);
            WriteElementString("LinksUpToDate", "false");
            WriteElementString("SharedDoc", "false");
            WriteElementString("HyperlinksChanged", "false");
            WriteElementString("AppVersion", AppVersion);
        }
    }
}
