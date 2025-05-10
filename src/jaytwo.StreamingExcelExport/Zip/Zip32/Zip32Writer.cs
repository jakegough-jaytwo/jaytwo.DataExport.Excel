using System;
using System.IO;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32Writer : ZipWriter
{
    public Zip32Writer(Stream outputStream, bool leaveOpen = false)
        : base(outputStream, leaveOpen)
    {
    }

    protected override IZipPart BuildLocalFileHeader(ushort compressionMethod, string fileName)
        => Zip32LocalFileHeader.CreateDefault(compressionMethod, fileName);

    protected override IZipPart BuildDataDescriptor(uint crc32, long compressedSize, long uncompressedSize)
        => Zip32DataDescriptor.CreateDefault(crc32: crc32, compressedSize: (ushort)compressedSize, uncompressedSize: (ushort)uncompressedSize);

    protected override IZipPart BuildCentralDirectoryEntry(
        ushort compressionMethod,
        string fileName,
        long compressedSize,
        long uncompressedSize,
        string? comment,
        uint crc32,
        long localHeaderOffset)
        => Zip32CentralDirectoryEntry.CreateDefault(
            compressionMethod: compressionMethod,
            fileName: fileName,
            compressedSize: (uint)compressedSize,
            uncompressedSize: (uint)uncompressedSize,
            localHeaderOffset: (uint)localHeaderOffset,
            crc32: crc32,
            fileComment: comment);

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
