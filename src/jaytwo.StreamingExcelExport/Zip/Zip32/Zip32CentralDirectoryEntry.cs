using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32CentralDirectoryEntry : IZipPart
{
    public const uint KnownSignature = 0x02014b50;

    public uint? Signature { get; set; }

    public ushort? VersionMadeBy { get; set; }

    public ushort? VersionNeededToExtract { get; set; }

    public ushort? GeneralPurposeBitFlag { get; set; }

    public ushort? CompressionMethod { get; set; }

    public ushort? LastModTime { get; set; }

    public ushort? LastModDate { get; set; }

    public ushort? DiskNumberStart { get; set; }

    public ushort? InternalFileAttributes { get; set; }

    public uint? ExternalFileAttributes { get; set; }

    public uint? Crc32 { get; set; }

    public uint? CompressedSize { get; set; }

    public uint? UncompressedSize { get; set; }

    public ushort? FileNameLength { get; set; } // keeping as discrete property to better test parsing

    public string? FileName { get; set; }

    public uint? LocalHeaderOffset { get; set; }

    public ushort? ExtraFieldLength { get; set; }

    public ushort? FileCommentLength { get; set; } // keeping as discrete property to better test parsing

    public byte[]? ExtraField { get; set; }

    public string? FileComment { get; set; }

    public void WriteTo(Stream stream, bool validate = true)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        var fileNameBytes = Encoding.UTF8.GetBytes(ThrowIfNull(x => FileName) ?? string.Empty);
        if (validate && FileNameLength != fileNameBytes.Length)
        {
            throw new InvalidOperationException($"{nameof(FileNameLength)} must be equal to length of {nameof(FileName)}");
        }

        var commentBytes = Encoding.UTF8.GetBytes(FileComment ?? string.Empty);
        if (validate && FileCommentLength != commentBytes.Length)
        {
            throw new InvalidOperationException($"{nameof(FileCommentLength)} must be equal to length of {nameof(FileComment)}");
        }

        var extraFieldBytes = ExtraField ?? Array.Empty<byte>();
        if (validate && ExtraFieldLength != extraFieldBytes.Length)
        {
            throw new InvalidOperationException($"{nameof(ExtraFieldLength)} must be equal to length of {nameof(ExtraField)}");
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(ThrowIfNull(x => x.Signature) ?? default);
        writer.Write(ThrowIfNull(x => x.VersionMadeBy) ?? default);
        writer.Write(ThrowIfNull(x => x.VersionNeededToExtract) ?? default);
        writer.Write(ThrowIfNull(x => x.GeneralPurposeBitFlag) ?? default);
        writer.Write(ThrowIfNull(x => x.CompressionMethod) ?? default);
        writer.Write(ThrowIfNull(x => x.LastModTime) ?? default);
        writer.Write(ThrowIfNull(x => x.LastModDate) ?? default);
        writer.Write(ThrowIfNull(x => x.Crc32) ?? default);
        writer.Write(ThrowIfNull(x => x.CompressedSize) ?? default);
        writer.Write(ThrowIfNull(x => x.UncompressedSize) ?? default);
        writer.Write(ThrowIfNull(x => x.FileNameLength) ?? default);
        writer.Write(ThrowIfNull(x => x.ExtraFieldLength) ?? default);
        writer.Write(ThrowIfNull(x => x.FileCommentLength) ?? default);
        writer.Write(ThrowIfNull(x => x.DiskNumberStart) ?? default);
        writer.Write(ThrowIfNull(x => x.InternalFileAttributes) ?? default);
        writer.Write(ThrowIfNull(x => x.ExternalFileAttributes) ?? default);
        writer.Write(ThrowIfNull(x => x.LocalHeaderOffset) ?? default);
        writer.Write(fileNameBytes);
        writer.Write(extraFieldBytes);
        writer.Write(commentBytes);

        TValue ThrowIfNull<TValue>(Expression<Func<Zip32CentralDirectoryEntry, TValue>> propertyExpression)
            => ValidationHelper.EnsureNotNull(this, propertyExpression, validate);
    }

    public override string ToString() =>
        $"CDE32[\"{FileName}\", CRC={Crc32}, Size={CompressedSize}, Offset={LocalHeaderOffset}]";

    internal static bool TryParse(byte[] bytes, out Zip32CentralDirectoryEntry result, int offset = 0)
    {
        result = new Zip32CentralDirectoryEntry();
        return TryLoad(result, bytes, offset);
    }

    internal static Zip32CentralDirectoryEntry Parse(byte[] bytes, int offset = 0)
    {
        var result = new Zip32CentralDirectoryEntry();
        Load(result, bytes, offset);
        return result;
    }

    protected static bool TryLoad(Zip32CentralDirectoryEntry result, byte[] bytes, int offset = 0)
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

    protected static void Load(Zip32CentralDirectoryEntry result, byte[] bytes, int offset = 0)
    {
        // not validating byte length here, it's still useful to parse as much as we can for debugging
        result.Signature = BitConverter.ToUInt32(bytes, offset + Offsets.SignatureOffset);
        result.VersionMadeBy = BitConverter.ToUInt16(bytes, offset + Offsets.VersionMadeByOffset);
        result.VersionNeededToExtract = BitConverter.ToUInt16(bytes, offset + Offsets.VersionNeededToExtractOffset);
        result.GeneralPurposeBitFlag = BitConverter.ToUInt16(bytes, offset + Offsets.GeneralPurposeBitFlagOffset);
        result.CompressionMethod = BitConverter.ToUInt16(bytes, offset + Offsets.CompressionMethodOffset);
        result.LastModTime = BitConverter.ToUInt16(bytes, offset + Offsets.LastModTimeOffset);
        result.LastModDate = BitConverter.ToUInt16(bytes, offset + Offsets.LastModDateOffset);
        result.Crc32 = BitConverter.ToUInt32(bytes, offset + Offsets.CrcOffset);
        result.CompressedSize = BitConverter.ToUInt32(bytes, offset + Offsets.CompressedSizeOffset);
        result.UncompressedSize = BitConverter.ToUInt32(bytes, offset + Offsets.UncompressedSizeOffset);
        result.FileNameLength = BitConverter.ToUInt16(bytes, offset + Offsets.FileNameLengthOffset);
        result.ExtraFieldLength = BitConverter.ToUInt16(bytes, offset + Offsets.ExtraFieldLengthOffset);
        result.FileCommentLength = BitConverter.ToUInt16(bytes, offset + Offsets.CommentLengthOffset);
        result.DiskNumberStart = BitConverter.ToUInt16(bytes, offset + Offsets.DiskNumberStartOffset);
        result.InternalFileAttributes = BitConverter.ToUInt16(bytes, offset + Offsets.InternalFileAttributesOffset);
        result.ExternalFileAttributes = BitConverter.ToUInt32(bytes, offset + Offsets.ExternalFileAttributesOffset);
        result.LocalHeaderOffset = BitConverter.ToUInt32(bytes, offset + Offsets.LocalHeaderOffsetOffset);
        result.FileName = Encoding.UTF8.GetString(bytes, offset + Offsets.FileNameOffset, (int)result.FileNameLength);

        int extraFieldOffset = offset + Offsets.GetExtraFieldOffset(result.FileNameLength.Value);
        result.ExtraField = new byte[result.ExtraFieldLength.Value];
        Buffer.BlockCopy(bytes, extraFieldOffset, result.ExtraField, 0, result.ExtraFieldLength.Value);

        int commentOffset = offset + Offsets.GetCommentOffset(result.FileNameLength.Value, result.ExtraFieldLength.Value);
        result.FileComment = Encoding.UTF8.GetString(bytes, commentOffset, (int)result.FileCommentLength);
    }

    internal static class Offsets
    {
        public const int SignatureOffset = 0;
        public const int VersionMadeByOffset = SignatureOffset + 4;
        public const int VersionNeededToExtractOffset = VersionMadeByOffset + 2;
        public const int GeneralPurposeBitFlagOffset = VersionNeededToExtractOffset + 2;
        public const int CompressionMethodOffset = GeneralPurposeBitFlagOffset + 2;
        public const int LastModTimeOffset = CompressionMethodOffset + 2;
        public const int LastModDateOffset = LastModTimeOffset + 2;
        public const int CrcOffset = LastModDateOffset + 2;
        public const int CompressedSizeOffset = CrcOffset + 4;
        public const int UncompressedSizeOffset = CompressedSizeOffset + 4;
        public const int FileNameLengthOffset = UncompressedSizeOffset + 4;
        public const int ExtraFieldLengthOffset = FileNameLengthOffset + 2;
        public const int CommentLengthOffset = ExtraFieldLengthOffset + 2;
        public const int DiskNumberStartOffset = CommentLengthOffset + 2;
        public const int InternalFileAttributesOffset = DiskNumberStartOffset + 2;
        public const int ExternalFileAttributesOffset = InternalFileAttributesOffset + 2;
        public const int LocalHeaderOffsetOffset = ExternalFileAttributesOffset + 4;
        public const int FileNameOffset = LocalHeaderOffsetOffset + 4;

        public static int GetCommentOffset(ushort fileNameLength, ushort extraFieldLength)
            => GetExtraFieldOffset(fileNameLength) + extraFieldLength;

        public static int GetExtraFieldOffset(ushort fileNameLength)
            => FileNameOffset + (int)fileNameLength;
    }
}
