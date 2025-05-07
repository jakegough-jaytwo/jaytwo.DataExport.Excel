using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32LocalFileHeader : IZipPart
{
    public const uint Signature = 0x04034b50;
    private const ushort VersionNeededToExtract = ZipConstants.Versions.Version20;
    private const ushort GeneralPurposeBitFlag = ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows;
    private const ushort CompressionMethod = ZipConstants.CompressionMethods.NoCompression;
    private const ushort LastModTime = 0;
    private const ushort LastModDate = 0;
    private const uint Crc32Placeholder = 0;
    private const uint CompressedSizePlaceholder = 0;
    private const uint UncompressedSizePlaceholder = 0;

    public string FileName { get; set; } = string.Empty;

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        var fileNameBytes = Encoding.UTF8.GetBytes(FileName);

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(Signature);                          // 0x04034b50
        writer.Write(VersionNeededToExtract);             // version needed
        writer.Write(GeneralPurposeBitFlag);              // flags (bit 3 = data descriptor)
        writer.Write(CompressionMethod);                  // method (0 = store)
        writer.Write(LastModTime);                        // time
        writer.Write(LastModDate);                        // date
        writer.Write(Crc32Placeholder);                   // placeholder
        writer.Write(CompressedSizePlaceholder);          // placeholder
        writer.Write(UncompressedSizePlaceholder);        // placeholder
        writer.Write((ushort)fileNameBytes.Length);       // file name length
        writer.Write((ushort)0);                          // extra field length
        writer.Write(fileNameBytes);                      // file name
    }
}
