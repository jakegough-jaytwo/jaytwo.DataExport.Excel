using System;
using System.IO;
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

    public static Zip32CentralDirectoryEntry CreateDefault(
        ushort compressionMethod,
        uint? crc32 = default,
        uint? compressedSize = default,
        uint? uncompressedSize = default,
        string? fileName = default,
        string? fileComment = default,
        uint? localHeaderOffset = default)
    {
        var result = new Zip32CentralDirectoryEntry()
        {
            Signature = KnownSignature,
            VersionMadeBy = ZipConstants.Versions.Version20,
            VersionNeededToExtract = ZipConstants.Versions.Version20,
            GeneralPurposeBitFlag = ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows,
            CompressionMethod = compressionMethod,
            LastModTime = 0,
            LastModDate = 0,
            DiskNumberStart = 0,
            InternalFileAttributes = 0,
            ExternalFileAttributes = 0,
            Crc32 = crc32,
            CompressedSize = compressedSize,
            UncompressedSize = uncompressedSize,
            ExtraFieldLength = 0,
            ExtraField = Array.Empty<byte>(),
        };

        if (!string.IsNullOrEmpty(fileName))
        {
            result.FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            result.FileNameLength = (ushort)Encoding.UTF8.GetByteCount(fileName);
        }

        fileComment ??= string.Empty;
        result.FileComment = fileComment;
        result.FileCommentLength = (ushort)Encoding.UTF8.GetByteCount(fileComment);

        result.LocalHeaderOffset = localHeaderOffset;

        return result;
    }

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        var fileNameBytes = Encoding.UTF8.GetBytes(FileName ?? throw new ArgumentNullException(nameof(FileName)));
        if (FileNameLength != fileNameBytes.Length)
        {
            throw new ArgumentException("FileNameLength must be equal to length of file name", nameof(FileNameLength));
        }

        var commentBytes = Encoding.UTF8.GetBytes(FileComment ?? string.Empty);
        if (FileCommentLength != commentBytes.Length)
        {
            throw new ArgumentException("FileCommentLength must be equal to length of file comment", nameof(FileCommentLength));
        }

        var extraFieldBytes = ExtraField ?? Array.Empty<byte>();
        if (ExtraFieldLength != extraFieldBytes.Length)
        {
            throw new ArgumentException("FileCommentLength must be equal to length of file comment", nameof(FileCommentLength));
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(Signature ?? throw new InvalidOperationException($"{nameof(Signature)} is required."));
        writer.Write(VersionMadeBy ?? throw new InvalidOperationException($"{nameof(VersionMadeBy)} is required."));
        writer.Write(VersionNeededToExtract ?? throw new InvalidOperationException($"{nameof(VersionNeededToExtract)} is required."));
        writer.Write(GeneralPurposeBitFlag ?? throw new InvalidOperationException($"{nameof(GeneralPurposeBitFlag)} is required."));
        writer.Write(CompressionMethod ?? throw new InvalidOperationException($"{nameof(CompressionMethod)} is required."));
        writer.Write(LastModTime ?? throw new InvalidOperationException($"{nameof(LastModTime)} is required."));
        writer.Write(LastModDate ?? throw new InvalidOperationException($"{nameof(LastModDate)} is required."));
        writer.Write(Crc32 ?? throw new InvalidOperationException($"{nameof(Crc32)} is required."));
        writer.Write(CompressedSize ?? throw new InvalidOperationException($"{nameof(CompressedSize)} is required."));
        writer.Write(UncompressedSize ?? throw new InvalidOperationException($"{nameof(UncompressedSize)} is required."));
        writer.Write(FileNameLength ?? throw new InvalidOperationException($"{nameof(FileNameLength)} is required."));
        writer.Write(ExtraFieldLength ?? throw new InvalidOperationException($"{nameof(ExtraFieldLength)} is required."));
        writer.Write(FileCommentLength ?? throw new InvalidOperationException($"{nameof(FileCommentLength)} is required."));
        writer.Write(DiskNumberStart ?? throw new InvalidOperationException($"{nameof(DiskNumberStart)} is required."));
        writer.Write(InternalFileAttributes ?? throw new InvalidOperationException($"{nameof(InternalFileAttributes)} is required."));
        writer.Write(ExternalFileAttributes ?? throw new InvalidOperationException($"{nameof(ExternalFileAttributes)} is required."));
        writer.Write(LocalHeaderOffset ?? throw new InvalidOperationException($"{nameof(LocalHeaderOffset)} is required."));
        writer.Write(fileNameBytes);
        writer.Write(extraFieldBytes);
        writer.Write(commentBytes);
    }

    public override string ToString() =>
        $"CDE32[\"{FileName}\", CRC={Crc32}, Size={CompressedSize}, Offset={LocalHeaderOffset}]";

    internal static Zip32CentralDirectoryEntry Parse(byte[] bytes, int offset = 0)
    {
        var result = new Zip32CentralDirectoryEntry();
        Load(result, bytes, offset);
        return result;
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
