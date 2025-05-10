using System;
using System.IO;
using System.Text;
using jaytwo.StreamingExcelExport.Zip.Zip32;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64DataDescriptor : IZipPart
{
    // ZIP64 Data Descriptor uses the same signature as ZIP32
    public const uint KnownSignature = Zip32DataDescriptor.KnownSignature;

    public uint? Signature { get; set; }

    public uint? Crc32 { get; set; }

    public ulong? CompressedSize { get; set; }

    public ulong? UncompressedSize { get; set; }

    public static Zip64DataDescriptor CreateDefault(
        uint? crc32 = default,
        ulong? compressedSize = default,
        ulong? uncompressedSize = default)
    {
        return new Zip64DataDescriptor
        {
            Signature = KnownSignature,
            Crc32 = crc32,
            CompressedSize = compressedSize,
            UncompressedSize = uncompressedSize,
        };
    }

    public static Zip64DataDescriptor Parse(byte[] bytes)
    {
        // not validating byte length here, it's still useful to parse as much as we can for debugging

        var result = new Zip64DataDescriptor();
        result.Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset);
        result.Crc32 = BitConverter.ToUInt32(bytes, Offsets.CrcOffset);
        result.CompressedSize = BitConverter.ToUInt64(bytes, Offsets.CompressedSizeOffset);
        result.UncompressedSize = BitConverter.ToUInt64(bytes, Offsets.UncompressedSizeOffset);

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
        $"DD64[CRC={Crc32}, Size={CompressedSize}]";

    public static class Offsets
    {
        public const int SignatureOffset = 0;
        public const int CrcOffset = SignatureOffset + 4;
        public const int CompressedSizeOffset = CrcOffset + 4;
        public const int UncompressedSizeOffset = CompressedSizeOffset + 8;
    }
}
