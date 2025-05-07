using System;
using System.IO;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32Writer : ZipWriter
{
    public Zip32Writer(Stream outputStream, bool leaveOpen = false)
        : base(outputStream, leaveOpen)
    {
    }

    protected override IZipPart BuildLocalFileHeader(string fileName)
        => new Zip32LocalFileHeader { FileName = fileName };

    protected override IZipPart BuildDataDescriptor(uint crc32, long fileLength)
        => new Zip32DataDescriptor { Crc32 = crc32, UncompressedSize = (uint)fileLength, };

    protected override IZipPart BuildCentralDirectoryEntry(string fileName, long uncompressedSize, string comment, uint crc32, long localHeaderOffset)
        => new Zip32CentralDirectoryEntry
        {
            FileName = fileName,
            UncompressedSize = (uint)uncompressedSize,
            LocalHeaderOffset = (uint)localHeaderOffset,
            Crc32 = crc32,
            FileComment = comment,
        };

    protected override void WriteZipCentralDirectory()
    {
        WriteZipCentralDirectoryEntries(out var totalEntries, out var centralDirectoryStart, out var centralDirectoryEnd);

        WriteZip32EndOfCentralDirectory(
            totalEntries: totalEntries,
            centralDirectoryOffset: centralDirectoryStart,
            centralDirectorySize: centralDirectoryEnd - centralDirectoryStart,
            comment: string.Empty);
    }
}
