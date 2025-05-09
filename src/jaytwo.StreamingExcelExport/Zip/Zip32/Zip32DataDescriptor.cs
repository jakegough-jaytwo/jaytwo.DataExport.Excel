using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32DataDescriptor : IZipPart
{
    public const uint Signature = 0x08074b50;

    public uint Crc32 { get; set; }

    public uint CompressedSize { get; set; }

    public uint UncompressedSize { get; set; }

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(Signature);
        writer.Write(Crc32);
        writer.Write(CompressedSize);
        writer.Write(UncompressedSize);
    }
}
