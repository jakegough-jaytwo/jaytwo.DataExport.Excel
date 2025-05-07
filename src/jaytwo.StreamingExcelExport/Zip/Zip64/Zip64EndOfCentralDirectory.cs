using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64EndOfCentralDirectory : IZipPart
{
    public const uint Signature = 0x06064b50;
    private const ulong SizeOfEOCD = 44; // Size of remaining EOCD data (fixed size with no extensible data)
    private const ushort VersionMadeBy = ZipConstants.Versions.Version45;
    private const ushort VersionNeededToExtract = VersionMadeBy;
    private const uint DiskNumber = 0;
    private const uint CentralDirectoryStartDisk = 0;

    public ulong TotalEntriesOnThisDisk => TotalEntries;

    public ulong TotalEntries { get; set; }

    public ulong CentralDirectorySize { get; set; }

    public ulong CentralDirectoryOffset { get; set; }

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(Signature);
        writer.Write(SizeOfEOCD);
        writer.Write(VersionMadeBy);
        writer.Write(VersionNeededToExtract);
        writer.Write(DiskNumber);
        writer.Write(CentralDirectoryStartDisk);
        writer.Write(TotalEntriesOnThisDisk);
        writer.Write(TotalEntries);
        writer.Write(CentralDirectorySize);
        writer.Write(CentralDirectoryOffset);
    }
}
