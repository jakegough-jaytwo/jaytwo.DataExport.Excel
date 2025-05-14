using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace jaytwo.DataExport.Excel.Zip.Zip64;

internal class Zip64EndOfCentralDirectory : IZipPart
{
    public const uint KnownSignature = 0x06064b50;
    public const ulong FixedSizeOfEOCD = 44;

    public uint? Signature { get; set; }

    public ulong? SizeOfEOCD { get; set; }

    public ushort? VersionMadeBy { get; set; }

    public ushort? VersionNeededToExtract { get; set; }

    public uint? DiskNumber { get; set; }

    public uint? CentralDirectoryStartDisk { get; set; }

    public ulong? TotalEntriesOnThisDisk { get; set; }

    public ulong? TotalEntries { get; set; }

    public ulong? CentralDirectorySize { get; set; }

    public ulong? CentralDirectoryOffset { get; set; }

    public void WriteTo(Stream stream, bool validate = true)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(ThrowIfNull(x => x.Signature) ?? default);
        writer.Write(ThrowIfNull(x => x.SizeOfEOCD) ?? default);
        writer.Write(ThrowIfNull(x => x.VersionMadeBy) ?? default);
        writer.Write(ThrowIfNull(x => x.VersionNeededToExtract) ?? default);
        writer.Write(ThrowIfNull(x => x.DiskNumber) ?? default);
        writer.Write(ThrowIfNull(x => x.CentralDirectoryStartDisk) ?? default);
        writer.Write(ThrowIfNull(x => x.TotalEntriesOnThisDisk) ?? default);
        writer.Write(ThrowIfNull(x => x.TotalEntries) ?? default);
        writer.Write(ThrowIfNull(x => x.CentralDirectorySize) ?? default);
        writer.Write(ThrowIfNull(x => x.CentralDirectoryOffset) ?? default);

        TValue ThrowIfNull<TValue>(Expression<Func<Zip64EndOfCentralDirectory, TValue>> propertyExpression)
            => ValidationHelper.EnsureNotNull(this, propertyExpression, validate);
    }

    public override string ToString()
        => $"EOCD64[Entries={TotalEntries}, Size={CentralDirectorySize}, Offset={CentralDirectoryOffset}]";

    internal static bool TryParse(byte[] bytes, out Zip64EndOfCentralDirectory result, int offset = 0)
    {
        result = new Zip64EndOfCentralDirectory();
        return TryLoad(result, bytes, offset);
    }

    internal static Zip64EndOfCentralDirectory Parse(byte[] bytes, int offset = 0)
    {
        var result = new Zip64EndOfCentralDirectory();
        Load(result, bytes, offset);
        return result;
    }

    protected static bool TryLoad(Zip64EndOfCentralDirectory result, byte[] bytes, int offset = 0)
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

    protected static void Load(Zip64EndOfCentralDirectory result, byte[] bytes, int offset = 0)
    {
        result.Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset);
        result.SizeOfEOCD = BitConverter.ToUInt64(bytes, Offsets.SizeOfEOCDOffset);
        result.VersionMadeBy = BitConverter.ToUInt16(bytes, Offsets.VersionMadeByOffset);
        result.VersionNeededToExtract = BitConverter.ToUInt16(bytes, Offsets.VersionNeededToExtractOffset);
        result.DiskNumber = BitConverter.ToUInt32(bytes, Offsets.DiskNumberOffset);
        result.CentralDirectoryStartDisk = BitConverter.ToUInt32(bytes, Offsets.CentralDirectoryStartDiskOffset);
        result.TotalEntriesOnThisDisk = BitConverter.ToUInt64(bytes, Offsets.TotalEntriesOnThisDiskOffset);
        result.TotalEntries = BitConverter.ToUInt64(bytes, Offsets.TotalEntriesOffset);
        result.CentralDirectorySize = BitConverter.ToUInt64(bytes, Offsets.CentralDirectorySizeOffset);
        result.CentralDirectoryOffset = BitConverter.ToUInt64(bytes, Offsets.CentralDirectoryOffsetOffset);
    }

    internal static class Offsets
    {
        public const int SignatureOffset = 0;
        public const int SizeOfEOCDOffset = SignatureOffset + 4;
        public const int VersionMadeByOffset = SizeOfEOCDOffset + 8;
        public const int VersionNeededToExtractOffset = VersionMadeByOffset + 2;
        public const int DiskNumberOffset = VersionNeededToExtractOffset + 2;
        public const int CentralDirectoryStartDiskOffset = DiskNumberOffset + 4;
        public const int TotalEntriesOnThisDiskOffset = CentralDirectoryStartDiskOffset + 4;
        public const int TotalEntriesOffset = TotalEntriesOnThisDiskOffset + 8;
        public const int CentralDirectorySizeOffset = TotalEntriesOffset + 8;
        public const int CentralDirectoryOffsetOffset = CentralDirectorySizeOffset + 8;
    }
}
