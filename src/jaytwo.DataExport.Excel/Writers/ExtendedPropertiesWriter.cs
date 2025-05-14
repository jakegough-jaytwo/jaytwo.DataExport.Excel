using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class ExtendedPropertiesWriter : XmlDocumentWriter
{
    public ExtendedPropertiesWriter(ExtendedPropertiesWriterContext context, XmlWriter writer)
        : base(writer)
    {
        Context = context;
    }

    public ExtendedPropertiesWriterContext Context { get; }

    protected override async Task WriteRootElementAsync(CancellationToken cancellationToken)
    {
        await using (CreateElementScope("Properties", "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties"))
        {
            WriteAttributeString("xmlns", "vt", null, Namespaces.vt);

            WriteElementString("Application", Context.Application);
            WriteElementString("DocSecurity", "0");
            WriteElementString("ScaleCrop", "false");

            await using (CreateElementScope("HeadingPairs"))
            {
                await using (CreateElementScope("vt", "vector", Namespaces.vt))
                {
                    WriteAttributeString("size", "2");
                    WriteAttributeString("baseType", "variant");

                    await using (CreateElementScope("vt", "variant", Namespaces.vt))
                    {
                        await WriteElementStringAsync("vt", "lpstr", Namespaces.vt, "Worksheets");
                    }

                    await using (CreateElementScope("vt", "variant", Namespaces.vt))
                    {
                        await WriteElementStringAsync("vt", "i4", Namespaces.vt, $"{Context.SheetNames.Count}");
                    }
                }
            }

            await using (CreateElementScope("TitlesOfParts"))
            {
                await using (CreateElementScope("vt", "vector", Namespaces.vt))
                {
                    WriteAttributeString("size", $"{Context.SheetNames.Count}");
                    WriteAttributeString("baseType", "lpstr");

                    foreach (var sheetName in Context.SheetNames)
                    {
                        await WriteElementStringAsync("vt", "lpstr", Namespaces.vt, sheetName);
                    }
                }
            }

            WriteElementString("Company", Context.Company);
            WriteElementString("LinksUpToDate", "false");
            WriteElementString("SharedDoc", "false");
            WriteElementString("HyperlinksChanged", "false");
            WriteElementString("AppVersion", Context.AppVersion);
        }
    }

    private class Namespaces
    {
#pragma warning disable SA1303 // Const field names should begin with upper-case letter
        public const string vt = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";
#pragma warning restore SA1303 // Const field names should begin with upper-case letter
    }
}
