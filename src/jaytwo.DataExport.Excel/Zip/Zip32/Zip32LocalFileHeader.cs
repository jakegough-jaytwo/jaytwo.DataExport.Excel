using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using static jaytwo.DataExport.Excel.Zip.ZipConstants;

namespace jaytwo.DataExport.Excel.Zip.Zip32;

internal class Zip32LocalFileHeader : IZipPart
{
    public const uint KnownSignature = 0x04034b50;

    public uint? Signature { get; set; }

    public ushort? VersionNeededToExtract { get; set; }

    public ushort? GeneralPurposeBitFlag { get; set; }

    public ushort? CompressionMethod { get; set; }

    public ushort? LastModTime { get; set; }

    public ushort? LastModDate { get; set; }

    public uint? Crc32 { get; set; }

    public uint? CompressedSize { get; set; }

    public uint? UncompressedSize { get; set; }

    public ushort? FileNameLength { get; set; }

    public ushort? ExtraFieldLength { get; set; }

    public string? FileName { get; set; }

    public byte[]? ExtraField { get; set; }

    public static Zip32LocalFileHeader Parse(byte[] bytes)
    {
        var result = new Zip32LocalFileHeader();
        Load(result, bytes);
        return result;
    }

    public void WriteTo(Stream stream, bool validate = true)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        var fileNameBytes = Encoding.UTF8.GetBytes(ThrowIfNull(x => x.FileName) ?? string.Empty);
        if (validate && FileNameLength != fileNameBytes.Length)
        {
            throw new InvalidOperationException($"{nameof(FileNameLength)} must match actual byte length of {nameof(FileName)}.");
        }

        var extraFieldBytes = ExtraField ?? Array.Empty<byte>();
        if (validate && ExtraFieldLength != extraFieldBytes.Length)
        {
            throw new InvalidOperationException($"{nameof(ExtraFieldLength)} must match length of {nameof(ExtraField)}.");
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(ThrowIfNull(x => Signature) ?? default);
        writer.Write(ThrowIfNull(x => VersionNeededToExtract) ?? default);
        writer.Write(ThrowIfNull(x => GeneralPurposeBitFlag) ?? default);
        writer.Write(ThrowIfNull(x => CompressionMethod) ?? default);
        writer.Write(ThrowIfNull(x => LastModTime) ?? default);
        writer.Write(ThrowIfNull(x => LastModDate) ?? default);
        writer.Write(ThrowIfNull(x => Crc32) ?? default);
        writer.Write(ThrowIfNull(x => CompressedSize) ?? default);
        writer.Write(ThrowIfNull(x => UncompressedSize) ?? default);
        writer.Write(ThrowIfNull(x => FileNameLength) ?? default);
        writer.Write(ThrowIfNull(x => ExtraFieldLength) ?? default);
        writer.Write(fileNameBytes);
        writer.Write(extraFieldBytes);

        TValue ThrowIfNull<TValue>(Expression<Func<Zip32LocalFileHeader, TValue>> propertyExpression)
            => ValidationHelper.EnsureNotNull(this, propertyExpression, validate);
    }

    public override string ToString() =>
        $"LFH32[\"{FileName}\", Size={CompressedSize}, CRC={Crc32}]";

    internal static bool TryParse(byte[] bytes, out Zip32LocalFileHeader result, int offset = 0)
    {
        result = new Zip32LocalFileHeader();
        return TryLoad(result, bytes, offset);
    }

    internal static Zip32LocalFileHeader Parse(byte[] bytes, int offset = 0)
    {
        var result = new Zip32LocalFileHeader();
        Load(result, bytes, offset);
        return result;
    }

    protected static bool TryLoad(Zip32LocalFileHeader result, byte[] bytes, int offset = 0)
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

    protected static void Load(Zip32LocalFileHeader result, byte[] bytes, int offset = 0)
    {
        result.Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset);
        result.VersionNeededToExtract = BitConverter.ToUInt16(bytes, Offsets.VersionNeededToExtractOffset);
        result.GeneralPurposeBitFlag = BitConverter.ToUInt16(bytes, Offsets.GeneralPurposeBitFlagOffset);
        result.CompressionMethod = BitConverter.ToUInt16(bytes, Offsets.CompressionMethodOffset);
        result.LastModTime = BitConverter.ToUInt16(bytes, Offsets.LastModTimeOffset);
        result.LastModDate = BitConverter.ToUInt16(bytes, Offsets.LastModDateOffset);
        result.Crc32 = BitConverter.ToUInt32(bytes, Offsets.CrcOffset);
        result.CompressedSize = BitConverter.ToUInt32(bytes, Offsets.CompressedSizeOffset);
        result.UncompressedSize = BitConverter.ToUInt32(bytes, Offsets.UncompressedSizeOffset);
        result.FileNameLength = BitConverter.ToUInt16(bytes, Offsets.FileNameLengthOffset);
        result.ExtraFieldLength = BitConverter.ToUInt16(bytes, Offsets.ExtraFieldLengthOffset);

        result.FileName = Encoding.UTF8.GetString(bytes, Offsets.FileNameOffset, result.FileNameLength.Value);

        int extraFieldOffset = Offsets.GetExtraFieldOffset(result.FileNameLength ?? 0);
        result.ExtraField = new byte[result.ExtraFieldLength.Value];
        Buffer.BlockCopy(bytes, extraFieldOffset, result.ExtraField, 0, result.ExtraFieldLength.Value);
    }

    internal static class Offsets
    {
        public const int SignatureOffset = 0;
        public const int VersionNeededToExtractOffset = SignatureOffset + 4;
        public const int GeneralPurposeBitFlagOffset = VersionNeededToExtractOffset + 2;
        public const int CompressionMethodOffset = GeneralPurposeBitFlagOffset + 2;
        public const int LastModTimeOffset = CompressionMethodOffset + 2;
        public const int LastModDateOffset = LastModTimeOffset + 2;
        public const int CrcOffset = LastModDateOffset + 2;
        public const int CompressedSizeOffset = CrcOffset + 4;
        public const int UncompressedSizeOffset = CompressedSizeOffset + 4;
        public const int FileNameLengthOffset = UncompressedSizeOffset + 4;
        public const int ExtraFieldLengthOffset = FileNameLengthOffset + 2;
        public const int FileNameOffset = ExtraFieldLengthOffset + 2;

        public static int GetExtraFieldOffset(int fileNameLength)
            => FileNameOffset + fileNameLength;
    }
}
