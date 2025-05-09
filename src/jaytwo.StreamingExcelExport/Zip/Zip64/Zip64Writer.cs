using System;
using System.IO;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64Writer : ZipWriter
{
    public Zip64Writer(Stream outputStream, bool leaveOpen = false)
        : base(outputStream, leaveOpen)
    {
    }

    protected override IZipPart BuildLocalFileHeader(string fileName)
        => new Zip64LocalFileHeader { FileName = fileName };

    protected override IZipPart BuildDataDescriptor(uint crc32, long compressedSize, long uncompressedSize)
        => new Zip64DataDescriptor { Crc32 = crc32, CompressedSize = (ulong)compressedSize, UncompressedSize = (ulong)uncompressedSize };

    protected override IZipPart BuildCentralDirectoryEntry(
        ushort compressionMethod,
        string fileName,
        long compressedSize,
        long uncompressedSize,
        string? comment,
        uint crc32,
        long localHeaderOffset)
        => Zip64CentralDirectoryEntry.CreateDefault(
            compressionMethod: compressionMethod,
            fileName: fileName,
            compressedSize: (ulong)compressedSize,
            uncompressedSize: (ulong)uncompressedSize,
            localHeaderOffset: (ulong)localHeaderOffset,
            crc32: crc32,
            fileComment: comment);

    protected override void WriteZipCentralDirectory()
    {
        /*
         * zip64: write entries, write Zip64EOCD, write Zip64EOCDLocator, write Legacy Zip32EOCD
         *
         * Most ZIP readers scan backward from the end of the file looking for:
         *   EOCD → if 0xFFFF/0xFFFFFFFF is found → fall back to:
         *   Locator → go to offset of:
         *   ZIP64 EOCD → read full ZIP64-compatible metadata
         */

        WriteZipCentralDirectoryEntries(out var totalEntries, out var centralDirectoryStart, out var centralDirectoryEnd);

        WriteZip64EndOfCentralDirectory(
            totalEntries: totalEntries,
            centralDirectoryOffset: centralDirectoryStart,
            centralDirectorySize: centralDirectoryEnd - centralDirectoryStart,
            comment: string.Empty,
            startPosition: out var zip64EndOfCentralDirectoryStart,
            endPosition: out _);

        WriteZip64EndOfCentralDirectoryLocator(zip64EndOfCentralDirectoryStart);

        WriteZip32EndOfCentralDirectory(
            totalEntries: 0xFFFF,
            centralDirectoryOffset: 0xFFFFFFFF,
            centralDirectorySize: 0xFFFFFFFF,
            comment: string.Empty);
    }

    protected void WriteZip64EndOfCentralDirectoryLocator(long zip64EndOfCentralDirectoryOffset)
        => WriteToOutput(new Zip64EndOfCentralDirectoryLocator { Zip64EndOfCentralDirectoryOffset = (ulong)zip64EndOfCentralDirectoryOffset });

    protected void WriteZip64EndOfCentralDirectory(int totalEntries, long centralDirectoryOffset, long centralDirectorySize, string comment, out long startPosition, out long endPosition)
        => WriteToOutput(
            new Zip64EndOfCentralDirectory
            {
                TotalEntries = (uint)totalEntries,
                CentralDirectoryOffset = (ulong)centralDirectoryOffset,
                CentralDirectorySize = (ulong)centralDirectorySize,
            },
            out startPosition,
            out endPosition);
}
