using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64EndOfCentralDirectoryLocator : IZipPart
{
    public const uint Signature = 0x07064b50;
    private const uint CentralDirectoryStartDisk = 0;
    private const uint TotalDisks = 1;

    public ulong Zip64EndOfCentralDirectoryOffset { get; set; } // Where the Zip64 EOCD starts

    public void WriteTo(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(Signature);
        writer.Write(CentralDirectoryStartDisk);
        writer.Write(Zip64EndOfCentralDirectoryOffset);
        writer.Write(TotalDisks);
    }
}
