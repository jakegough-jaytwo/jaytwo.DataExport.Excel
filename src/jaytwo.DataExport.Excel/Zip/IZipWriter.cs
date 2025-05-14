using System;
using System.IO;

namespace jaytwo.DataExport.Excel.Zip;

internal interface IZipWriter : IDisposable, IAsyncDisposable
{
    Stream OpenEntryStream(string fileName, string? comment = default);
}
