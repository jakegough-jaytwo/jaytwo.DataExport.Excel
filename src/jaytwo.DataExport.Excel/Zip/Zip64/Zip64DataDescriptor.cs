using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using jaytwo.DataExport.Excel.Zip.Zip32;

namespace jaytwo.DataExport.Excel.Zip.Zip64;

internal class Zip64DataDescriptor : IZipPart
{
    // ZIP64 Data Descriptor uses the same signature as ZIP32
    public const uint KnownSignature = Zip32DataDescriptor.KnownSignature;

    public uint? Signature { get; set; }

    public uint? Crc32 { get; set; }

    public ulong? CompressedSize { get; set; }

    public ulong? UncompressedSize { get; set; }

    public void WriteTo(Stream stream, bool validate = true)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(ThrowIfNull(x => x.Signature) ?? default);
        writer.Write(ThrowIfNull(x => x.Crc32) ?? default);
        writer.Write(ThrowIfNull(x => x.CompressedSize) ?? default);
        writer.Write(ThrowIfNull(x => x.UncompressedSize) ?? default);

        TValue ThrowIfNull<TValue>(Expression<Func<Zip64DataDescriptor, TValue>> propertyExpression)
            => ValidationHelper.EnsureNotNull(this, propertyExpression, validate);
    }

    public override string ToString() =>
        $"DD64[CRC={Crc32}, Size={CompressedSize}]";

    internal static bool TryParse(byte[] bytes, out Zip64DataDescriptor result, int offset = 0)
    {
        result = new Zip64DataDescriptor();
        return TryLoad(result, bytes, offset);
    }

    internal static Zip64DataDescriptor Parse(byte[] bytes, int offset = 0)
    {
        var result = new Zip64DataDescriptor();
        Load(result, bytes, offset);
        return result;
    }

    protected static bool TryLoad(Zip64DataDescriptor result, byte[] bytes, int offset = 0)
    {
        try
        {
            Load(result, bytes, offset);
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected static void Load(Zip64DataDescriptor result, byte[] bytes, int offset = 0)
    {
        // not validating byte length here, it's still useful to parse as much as we can for debugging

        result.Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset);
        result.Crc32 = BitConverter.ToUInt32(bytes, Offsets.CrcOffset);
        result.CompressedSize = BitConverter.ToUInt64(bytes, Offsets.CompressedSizeOffset);
        result.UncompressedSize = BitConverter.ToUInt64(bytes, Offsets.UncompressedSizeOffset);
    }

    public static class Offsets
    {
        public const int SignatureOffset = 0;
        public const int CrcOffset = SignatureOffset + 4;
        public const int CompressedSizeOffset = CrcOffset + 4;
        public const int UncompressedSizeOffset = CompressedSizeOffset + 8;
    }
}
