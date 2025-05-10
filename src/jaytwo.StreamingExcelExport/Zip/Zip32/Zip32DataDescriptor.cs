using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32DataDescriptor : IZipPart
{
    public const uint KnownSignature = 0x08074b50;

    public uint? Signature { get; set; }

    public uint? Crc32 { get; set; }

    public uint? CompressedSize { get; set; }

    public uint? UncompressedSize { get; set; }

    public static Zip32DataDescriptor CreateDefault(
        uint crc32 = default,
        uint compressedSize = default,
        uint uncompressedSize = default)
    {
        return new Zip32DataDescriptor
        {
            Signature = KnownSignature,
            Crc32 = crc32,
            CompressedSize = compressedSize,
            UncompressedSize = uncompressedSize,
        };
    }

    public static Zip32DataDescriptor Parse(byte[] bytes)
    {
        // not validating byte length here, it's still useful to parse as much as we can for debugging

        var result = new Zip32DataDescriptor();
        result.Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset);
        result.Crc32 = BitConverter.ToUInt32(bytes, Offsets.CrcOffset);
        result.CompressedSize = BitConverter.ToUInt32(bytes, Offsets.CompressedSizeOffset);
        result.UncompressedSize = BitConverter.ToUInt32(bytes, Offsets.UncompressedSizeOffset);

        return result;
    }

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(Signature ?? throw new InvalidOperationException($"{nameof(Signature)} is required."));
        writer.Write(Crc32 ?? throw new InvalidOperationException($"{nameof(Crc32)} is required."));
        writer.Write(CompressedSize ?? throw new InvalidOperationException($"{nameof(CompressedSize)} is required."));
        writer.Write(UncompressedSize ?? throw new InvalidOperationException($"{nameof(UncompressedSize)} is required."));
    }

    public override string ToString() =>
        $"DD32[CRC={Crc32}, Size={CompressedSize}]";

    public static class Offsets
    {
        public const int SignatureOffset = 0;
        public const int CrcOffset = SignatureOffset + 4;
        public const int CompressedSizeOffset = CrcOffset + 4;
        public const int UncompressedSizeOffset = CompressedSizeOffset + 4;
    }
}
