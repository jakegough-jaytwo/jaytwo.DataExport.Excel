using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32DataDescriptor : IZipPart
{
    public const uint Signature = 0x08074b50;

    public uint Crc32 { get; set; }

    public uint CompressedSize => UncompressedSize;

    public uint UncompressedSize { get; set; }

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(Signature);          // 0x08074b50
        writer.Write(Crc32);              // CRC-32 of uncompressed data
        writer.Write(CompressedSize);     // Compressed size (same as uncompressed for method 0)
        writer.Write(UncompressedSize);   // Uncompressed size
    }
}
