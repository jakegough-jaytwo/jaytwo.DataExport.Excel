using System;
using System.IO;
using System.Text;
using static jaytwo.StreamingExcelExport.Zip.ZipConstants;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

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

    public static Zip32LocalFileHeader CreateDefault(ushort compressionMethod, string fileName)
    {
        return new Zip32LocalFileHeader
        {
            Signature = KnownSignature,
            VersionNeededToExtract = ZipConstants.Versions.Version20,
            GeneralPurposeBitFlag = ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows,
            CompressionMethod = compressionMethod,
            LastModTime = 0,
            LastModDate = 0,
            Crc32 = 0,
            CompressedSize = 0,
            UncompressedSize = 0,
            FileName = fileName,
            FileNameLength = (ushort)Encoding.UTF8.GetByteCount(fileName ?? string.Empty),
            ExtraField = Array.Empty<byte>(),
            ExtraFieldLength = 0,
        };
    }

    public static Zip32LocalFileHeader Parse(byte[] bytes)
    {
        var result = new Zip32LocalFileHeader();
        Load(result, bytes);
        return result;
    }

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        var fileNameBytes = Encoding.UTF8.GetBytes(FileName ?? throw new InvalidOperationException($"{nameof(FileName)} is required."));
        if (FileNameLength != fileNameBytes.Length)
        {
            throw new ArgumentException($"{nameof(FileNameLength)} must match actual byte length of {nameof(FileName)}.");
        }

        var extraFieldBytes = ExtraField ?? Array.Empty<byte>();
        if (ExtraFieldLength != extraFieldBytes.Length)
        {
            throw new ArgumentException($"{nameof(ExtraFieldLength)} must match length of {nameof(ExtraField)}.");
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(Signature ?? throw new InvalidOperationException($"{nameof(Signature)} is required."));
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
        writer.Write(fileNameBytes);
        writer.Write(extraFieldBytes);
    }

    public override string ToString() =>
        $"LFH32[\"{FileName}\", Size={CompressedSize}, CRC={Crc32}]";

    internal static void Load(Zip32LocalFileHeader header, byte[] bytes)
    {
        header.Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset);
        header.VersionNeededToExtract = BitConverter.ToUInt16(bytes, Offsets.VersionNeededToExtractOffset);
        header.GeneralPurposeBitFlag = BitConverter.ToUInt16(bytes, Offsets.GeneralPurposeBitFlagOffset);
        header.CompressionMethod = BitConverter.ToUInt16(bytes, Offsets.CompressionMethodOffset);
        header.LastModTime = BitConverter.ToUInt16(bytes, Offsets.LastModTimeOffset);
        header.LastModDate = BitConverter.ToUInt16(bytes, Offsets.LastModDateOffset);
        header.Crc32 = BitConverter.ToUInt32(bytes, Offsets.CrcOffset);
        header.CompressedSize = BitConverter.ToUInt32(bytes, Offsets.CompressedSizeOffset);
        header.UncompressedSize = BitConverter.ToUInt32(bytes, Offsets.UncompressedSizeOffset);
        header.FileNameLength = BitConverter.ToUInt16(bytes, Offsets.FileNameLengthOffset);
        header.ExtraFieldLength = BitConverter.ToUInt16(bytes, Offsets.ExtraFieldLengthOffset);

        header.FileName = Encoding.UTF8.GetString(bytes, Offsets.FileNameOffset, header.FileNameLength.Value);

        int extraFieldOffset = Offsets.GetExtraFieldOffset(header.FileNameLength ?? 0);
        header.ExtraField = new byte[header.ExtraFieldLength.Value];
        Buffer.BlockCopy(bytes, extraFieldOffset, header.ExtraField, 0, header.ExtraFieldLength.Value);
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
