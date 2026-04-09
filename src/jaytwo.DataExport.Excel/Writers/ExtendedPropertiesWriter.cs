using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class ExtendedPropertiesWriter : XmlDocumentWriter
{
    private readonly string _application;
    private readonly string _appVersion;
    private readonly string? _company;
    private readonly IList<string> _sheetNames;

    public ExtendedPropertiesWriter(string application, string appVersion, string? company, IList<string> sheetNames)
    {
        _application = application;
        _appVersion = appVersion;
        _company = company;
        _sheetNames = sheetNames;
    }

    public override string ZipPackagePath => "docProps/app.xml";

    protected override async Task WriteRootElementAsync(XmlWriter writer, CancellationToken cancellationToken)
    {
        await using (CreateElementScope(writer, "Properties", "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties"))
        {
            WriteAttributeString(writer, "xmlns", "vt", null, Namespaces.vt);

            WriteElementString(writer, "Application", _application);
            WriteElementString(writer, "DocSecurity", "0");
            WriteElementString(writer, "ScaleCrop", "false");

            await using (CreateElementScope(writer, "HeadingPairs"))
            {
                await using (CreateElementScope(writer, "vt", "vector", Namespaces.vt))
                {
                    WriteAttributeString(writer, "size", "2");
                    WriteAttributeString(writer, "baseType", "variant");

                    await using (CreateElementScope(writer, "vt", "variant", Namespaces.vt))
                    {
                        await WriteElementStringAsync(writer, "vt", "lpstr", Namespaces.vt, "Worksheets");
                    }

                    await using (CreateElementScope(writer, "vt", "variant", Namespaces.vt))
                    {
                        await WriteElementStringAsync(writer, "vt", "i4", Namespaces.vt, $"{_sheetNames.Count}");
                    }
                }
            }

            await using (CreateElementScope(writer, "TitlesOfParts"))
            {
                await using (CreateElementScope(writer, "vt", "vector", Namespaces.vt))
                {
                    WriteAttributeString(writer, "size", $"{_sheetNames.Count}");
                    WriteAttributeString(writer, "baseType", "lpstr");

                    foreach (var sheetName in _sheetNames)
                    {
                        await WriteElementStringAsync(writer, "vt", "lpstr", Namespaces.vt, sheetName);
                    }
                }
            }

            WriteElementString(writer, "Company", _company);
            WriteElementString(writer, "LinksUpToDate", "false");
            WriteElementString(writer, "SharedDoc", "false");
            WriteElementString(writer, "HyperlinksChanged", "false");
            WriteElementString(writer, "AppVersion", _appVersion);
        }
    }

    private class Namespaces
    {
#pragma warning disable SA1303 // Const field names should begin with upper-case letter
        public const string vt = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";
#pragma warning restore SA1303 // Const field names should begin with upper-case letter
    }
}
