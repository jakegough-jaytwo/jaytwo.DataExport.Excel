using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using jaytwo.StreamingExcelExport.Zip.Zip32;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64LocalFileHeader : IZipPart
{
    public const uint Signature = Zip32LocalFileHeader.Signature;
    private const ushort Zip64ExtraFieldHeaderId = 0x0001;
    private const uint SeeZip64ExtraFields = 0xFFFFFFFF;
    private const ushort VersionNeededForZip64 = ZipConstants.Versions.Version45;
    private const ushort GeneralPurposeBitFlag = ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows;
    private const ushort CompressionMethod = ZipConstants.CompressionMethods.NoCompression;
    private const ushort LastModTime = 0;
    private const ushort LastModDate = 0;
    private const uint Crc32Placeholder = 0;
    private const ulong CompressedSizePlaceholder = 0;
    private const ulong UncompressedSizePlaceholder = 0;

    public string FileName { get; set; } = string.Empty;

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        var fileNameBytes = Encoding.UTF8.GetBytes(FileName);

        var extraFieldHeaderLength = 4; // 2 byte header id + 2 byte data size
        var extraFieldDataLength = 16; // 8 byte UncompressedSize + 8 byte CompressedSize
        var extraFieldTotalLength = extraFieldHeaderLength + extraFieldDataLength;

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(Signature);                      // 0x04034b50
        writer.Write(VersionNeededForZip64);          // ZIP64 minimum version
        writer.Write(GeneralPurposeBitFlag);          // flags
        writer.Write(CompressionMethod);              // method
        writer.Write(LastModTime);                    // mod time
        writer.Write(LastModDate);                    // mod date
        writer.Write(Crc32Placeholder);               // CRC32 (0 for now)
        writer.Write(SeeZip64ExtraFields);            // Compressed size (placeholder)
        writer.Write(SeeZip64ExtraFields);            // Uncompressed size (placeholder)
        writer.Write((ushort)fileNameBytes.Length);
        writer.Write((ushort)extraFieldTotalLength);
        writer.Write(fileNameBytes);

        // Write ZIP64 extra field
        writer.Write(Zip64ExtraFieldHeaderId);        // Header ID = 0x0001
        writer.Write((ushort)extraFieldDataLength);
        writer.Write(UncompressedSizePlaceholder);
        writer.Write(CompressedSizePlaceholder);
    }
}
