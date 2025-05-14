using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace jaytwo.DataExport.Excel.Zip.Zip32;

internal class Zip32DataDescriptor : IZipPart
{
    public const uint KnownSignature = 0x08074b50;

    public uint? Signature { get; set; }

    public uint? Crc32 { get; set; }

    public uint? CompressedSize { get; set; }

    public uint? UncompressedSize { get; set; }

    public void WriteTo(Stream stream, bool validate = true)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(ThrowIfNull(x => Signature) ?? default);
        writer.Write(ThrowIfNull(x => Crc32) ?? default);
        writer.Write(ThrowIfNull(x => CompressedSize) ?? default);
        writer.Write(ThrowIfNull(x => UncompressedSize) ?? default);

        TValue ThrowIfNull<TValue>(Expression<Func<Zip32DataDescriptor, TValue>> propertyExpression)
            => ValidationHelper.EnsureNotNull(this, propertyExpression, validate);
    }

    public override string ToString() =>
        $"DD32[CRC={Crc32}, Size={CompressedSize}]";

    internal static bool TryParse(byte[] bytes, out Zip32DataDescriptor result, int offset = 0)
    {
        result = new Zip32DataDescriptor();
        return TryLoad(result, bytes, offset);
    }

    internal static Zip32DataDescriptor Parse(byte[] bytes, int offset = 0)
    {
        var result = new Zip32DataDescriptor();
        Load(result, bytes, offset);
        return result;
    }

    protected static bool TryLoad(Zip32DataDescriptor result, byte[] bytes, int offset = 0)
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

    protected static void Load(Zip32DataDescriptor result, byte[] bytes, int offset = 0)
    {
        result.Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset);
        result.Crc32 = BitConverter.ToUInt32(bytes, Offsets.CrcOffset);
        result.CompressedSize = BitConverter.ToUInt32(bytes, Offsets.CompressedSizeOffset);
        result.UncompressedSize = BitConverter.ToUInt32(bytes, Offsets.UncompressedSizeOffset);
    }

    public static class Offsets
    {
        public const int SignatureOffset = 0;
        public const int CrcOffset = SignatureOffset + 4;
        public const int CompressedSizeOffset = CrcOffset + 4;
        public const int UncompressedSizeOffset = CompressedSizeOffset + 4;
    }
}
