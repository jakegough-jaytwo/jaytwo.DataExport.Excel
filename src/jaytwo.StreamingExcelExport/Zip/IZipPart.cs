using System;
using System.IO;

namespace jaytwo.StreamingExcelExport.Zip;

internal interface IZipPart
{
    void WriteTo(Stream stream, bool validate = true);
}
