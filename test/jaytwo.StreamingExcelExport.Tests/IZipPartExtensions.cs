using System.IO;
using jaytwo.StreamingExcelExport.Zip;

namespace jaytwo.StreamingExcelExport.Tests;

internal static class IZipPartExtensions
{
    public static byte[] GetBytes(this IZipPart part, bool validate = true)
    {
        using var ms = new MemoryStream();
        part.WriteTo(ms, validate);
        return ms.ToArray();
    }
}
