using System;
using System.IO;

namespace jaytwo.DataExport.Excel.Zip;

internal interface IZipPart
{
    void WriteTo(Stream stream, bool validate = true);
}
