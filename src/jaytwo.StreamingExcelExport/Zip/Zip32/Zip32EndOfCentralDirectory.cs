using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32EndOfCentralDirectory : IZipPart
{
    public const uint KnownSignature = 0x06054b50;

    public uint? Signature { get; set; }

    public ushort? DiskNumber { get; set; }

    public ushort? CentralDirectoryStartDisk { get; set; }

    public ushort? TotalEntriesOnThisDisk { get; set; }

    public ushort? TotalEntries { get; set; }

    public uint? CentralDirectorySize { get; set; }

    public uint? CentralDirectoryOffset { get; set; }

    public string? Comment { get; set; }

    public ushort? CommentLength { get; set; }

    public static Zip32EndOfCentralDirectory CreateDefault(
        ushort totalEntries = 0,
        uint centralDirectorySize = 0,
        uint centralDirectoryOffset = 0,
        string? comment = null)
    {
        comment ??= string.Empty;

        return new Zip32EndOfCentralDirectory
        {
            Signature = KnownSignature,
            DiskNumber = 0,
            CentralDirectoryStartDisk = 0,
            TotalEntriesOnThisDisk = totalEntries,
            TotalEntries = totalEntries,
            CentralDirectorySize = centralDirectorySize,
            CentralDirectoryOffset = centralDirectoryOffset,
            Comment = comment,
            CommentLength = (ushort)Encoding.UTF8.GetByteCount(comment),
        };
    }

    public static Zip32EndOfCentralDirectory Parse(byte[] bytes)
    {
        var result = new Zip32EndOfCentralDirectory
        {
            Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset),
            DiskNumber = BitConverter.ToUInt16(bytes, Offsets.DiskNumberOffset),
            CentralDirectoryStartDisk = BitConverter.ToUInt16(bytes, Offsets.CentralDirectoryStartDiskOffset),
            TotalEntriesOnThisDisk = BitConverter.ToUInt16(bytes, Offsets.TotalEntriesOnThisDiskOffset),
            TotalEntries = BitConverter.ToUInt16(bytes, Offsets.TotalEntriesOffset),
            CentralDirectorySize = BitConverter.ToUInt32(bytes, Offsets.CentralDirectorySizeOffset),
            CentralDirectoryOffset = BitConverter.ToUInt32(bytes, Offsets.CentralDirectoryOffsetOffset),
            CommentLength = BitConverter.ToUInt16(bytes, Offsets.CommentLengthOffset),
        };

        result.Comment = Encoding.UTF8.GetString(bytes, Offsets.CommentOffset, result.CommentLength.Value);

        return result;
    }

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        var commentBytes = Encoding.UTF8.GetBytes(Comment ?? string.Empty);
        if (CommentLength != commentBytes.Length)
        {
            throw new ArgumentException("CommentLength must match actual comment byte length", nameof(CommentLength));
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(Signature ?? throw new InvalidOperationException($"{nameof(Signature)} is required."));
        writer.Write(DiskNumber ?? throw new InvalidOperationException($"{nameof(DiskNumber)} is required."));
        writer.Write(CentralDirectoryStartDisk ?? throw new InvalidOperationException($"{nameof(CentralDirectoryStartDisk)} is required."));
        writer.Write(TotalEntriesOnThisDisk ?? throw new InvalidOperationException($"{nameof(TotalEntriesOnThisDisk)} is required."));
        writer.Write(TotalEntries ?? throw new InvalidOperationException($"{nameof(TotalEntries)} is required."));
        writer.Write(CentralDirectorySize ?? throw new InvalidOperationException($"{nameof(CentralDirectorySize)} is required."));
        writer.Write(CentralDirectoryOffset ?? throw new InvalidOperationException($"{nameof(CentralDirectoryOffset)} is required."));
        writer.Write(CommentLength ?? throw new InvalidOperationException($"{nameof(CommentLength)} is required."));
        writer.Write(commentBytes);
    }

    public override string ToString() =>
        $"EOCD32[Entries={TotalEntries}, Size={CentralDirectorySize}, Offset={CentralDirectoryOffset}]";

    internal static class Offsets
    {
        public const int SignatureOffset = 0;
        public const int DiskNumberOffset = SignatureOffset + 4;
        public const int CentralDirectoryStartDiskOffset = DiskNumberOffset + 2;
        public const int TotalEntriesOnThisDiskOffset = CentralDirectoryStartDiskOffset + 2;
        public const int TotalEntriesOffset = TotalEntriesOnThisDiskOffset + 2;
        public const int CentralDirectorySizeOffset = TotalEntriesOffset + 2;
        public const int CentralDirectoryOffsetOffset = CentralDirectorySizeOffset + 4;
        public const int CommentLengthOffset = CentralDirectoryOffsetOffset + 4;
        public const int CommentOffset = CommentLengthOffset + 2;
    }
}
