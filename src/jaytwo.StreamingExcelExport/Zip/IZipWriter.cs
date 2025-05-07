using System;
using System.IO;
using System.Threading.Tasks;

namespace jaytwo.StreamingExcelExport.Zip;

internal interface IZipWriter
    : IDisposable, IAsyncDisposable
{
    Task WriteFileAsync(string fileName, string comment, Func<Stream, Task> writeFileCallback);
}
