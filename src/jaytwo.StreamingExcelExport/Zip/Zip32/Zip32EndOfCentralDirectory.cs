using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32EndOfCentralDirectory : IZipPart
{
    public const uint Signature = 0x06054b50;

    private const ushort DiskNumber = 0;

    private const ushort CentralDirectoryStartDisk = 0;

    public ushort TotalEntriesOnThisDisk => TotalEntries;

    public ushort TotalEntries { get; set; }

    public uint CentralDirectoryOffset { get; set; }

    public uint CentralDirectorySize { get; set; }

    public string? Comment { get; set; }

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        var commentBytes = Encoding.UTF8.GetBytes(Comment ?? string.Empty);

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(Signature);                                 // 0x06054b50
        writer.Write(DiskNumber);                                // Disk number
        writer.Write(CentralDirectoryStartDisk);                 // Start disk
        writer.Write(TotalEntriesOnThisDisk);                    // # entries on this disk
        writer.Write(TotalEntries);                              // Total entries
        writer.Write(CentralDirectorySize);                      // Size of central dir
        writer.Write(CentralDirectoryOffset);                    // Offset of central dir
        writer.Write((ushort)commentBytes.Length);               // Comment length
        writer.Write(commentBytes);                              // Comment
    }
}
