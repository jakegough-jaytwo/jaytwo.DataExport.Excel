using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace jaytwo.DataExport.Excel.Zip.Zip32;

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

    public void WriteTo(Stream stream, bool validate = true)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        var commentBytes = Encoding.UTF8.GetBytes(Comment ?? string.Empty);
        if (validate && CommentLength != commentBytes.Length)
        {
            throw new InvalidOperationException($"{nameof(CommentLength)} must be equal to length of {nameof(Comment)}");
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(ThrowIfNull(x => Signature) ?? default);
        writer.Write(ThrowIfNull(x => DiskNumber) ?? default);
        writer.Write(ThrowIfNull(x => CentralDirectoryStartDisk) ?? default);
        writer.Write(ThrowIfNull(x => TotalEntriesOnThisDisk) ?? default);
        writer.Write(ThrowIfNull(x => TotalEntries) ?? default);
        writer.Write(ThrowIfNull(x => CentralDirectorySize) ?? default);
        writer.Write(ThrowIfNull(x => CentralDirectoryOffset) ?? default);
        writer.Write(ThrowIfNull(x => CommentLength) ?? default);
        writer.Write(commentBytes);

        TValue ThrowIfNull<TValue>(Expression<Func<Zip32EndOfCentralDirectory, TValue>> propertyExpression)
            => ValidationHelper.EnsureNotNull(this, propertyExpression, validate);
    }

    public override string ToString() =>
        $"EOCD32[Entries={TotalEntries}, Size={CentralDirectorySize}, Offset={CentralDirectoryOffset}]";

    internal static bool TryParse(byte[] bytes, out Zip32EndOfCentralDirectory result, int offset = 0)
    {
        result = new Zip32EndOfCentralDirectory();
        return TryLoad(result, bytes, offset);
    }

    internal static Zip32EndOfCentralDirectory Parse(byte[] bytes, int offset = 0)
    {
        var result = new Zip32EndOfCentralDirectory();
        Load(result, bytes, offset);
        return result;
    }

    protected static bool TryLoad(Zip32EndOfCentralDirectory result, byte[] bytes, int offset = 0)
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

    protected static void Load(Zip32EndOfCentralDirectory result, byte[] bytes, int offset = 0)
    {
        result.Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset);
        result.DiskNumber = BitConverter.ToUInt16(bytes, Offsets.DiskNumberOffset);
        result.CentralDirectoryStartDisk = BitConverter.ToUInt16(bytes, Offsets.CentralDirectoryStartDiskOffset);
        result.TotalEntriesOnThisDisk = BitConverter.ToUInt16(bytes, Offsets.TotalEntriesOnThisDiskOffset);
        result.TotalEntries = BitConverter.ToUInt16(bytes, Offsets.TotalEntriesOffset);
        result.CentralDirectorySize = BitConverter.ToUInt32(bytes, Offsets.CentralDirectorySizeOffset);
        result.CentralDirectoryOffset = BitConverter.ToUInt32(bytes, Offsets.CentralDirectoryOffsetOffset);
        result.CommentLength = BitConverter.ToUInt16(bytes, Offsets.CommentLengthOffset);

        result.Comment = Encoding.UTF8.GetString(bytes, Offsets.CommentOffset, result.CommentLength.Value);
    }

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
