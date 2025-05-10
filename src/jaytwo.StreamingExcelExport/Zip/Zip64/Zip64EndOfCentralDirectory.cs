using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

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

    public static Zip64EndOfCentralDirectory CreateDefault(
        ulong totalEntries = 0,
        ulong centralDirectorySize = 0,
        ulong centralDirectoryOffset = 0)
    {
        return new Zip64EndOfCentralDirectory
        {
            Signature = KnownSignature,
            SizeOfEOCD = FixedSizeOfEOCD,
            VersionMadeBy = ZipConstants.Versions.Version45,
            VersionNeededToExtract = ZipConstants.Versions.Version45,
            DiskNumber = 0,
            CentralDirectoryStartDisk = 0,
            TotalEntriesOnThisDisk = totalEntries,
            TotalEntries = totalEntries,
            CentralDirectorySize = centralDirectorySize,
            CentralDirectoryOffset = centralDirectoryOffset,
        };
    }

    public static Zip64EndOfCentralDirectory Parse(byte[] bytes)
    {
        var result = new Zip64EndOfCentralDirectory
        {
            Signature = BitConverter.ToUInt32(bytes, Offsets.SignatureOffset),
            SizeOfEOCD = BitConverter.ToUInt64(bytes, Offsets.SizeOfEOCDOffset),
            VersionMadeBy = BitConverter.ToUInt16(bytes, Offsets.VersionMadeByOffset),
            VersionNeededToExtract = BitConverter.ToUInt16(bytes, Offsets.VersionNeededToExtractOffset),
            DiskNumber = BitConverter.ToUInt32(bytes, Offsets.DiskNumberOffset),
            CentralDirectoryStartDisk = BitConverter.ToUInt32(bytes, Offsets.CentralDirectoryStartDiskOffset),
            TotalEntriesOnThisDisk = BitConverter.ToUInt64(bytes, Offsets.TotalEntriesOnThisDiskOffset),
            TotalEntries = BitConverter.ToUInt64(bytes, Offsets.TotalEntriesOffset),
            CentralDirectorySize = BitConverter.ToUInt64(bytes, Offsets.CentralDirectorySizeOffset),
            CentralDirectoryOffset = BitConverter.ToUInt64(bytes, Offsets.CentralDirectoryOffsetOffset),
        };

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
        writer.Write(SizeOfEOCD ?? throw new InvalidOperationException($"{nameof(SizeOfEOCD)} is required."));
        writer.Write(VersionMadeBy ?? throw new InvalidOperationException($"{nameof(VersionMadeBy)} is required."));
        writer.Write(VersionNeededToExtract ?? throw new InvalidOperationException($"{nameof(VersionNeededToExtract)} is required."));
        writer.Write(DiskNumber ?? throw new InvalidOperationException($"{nameof(DiskNumber)} is required."));
        writer.Write(CentralDirectoryStartDisk ?? throw new InvalidOperationException($"{nameof(CentralDirectoryStartDisk)} is required."));
        writer.Write(TotalEntriesOnThisDisk ?? throw new InvalidOperationException($"{nameof(TotalEntriesOnThisDisk)} is required."));
        writer.Write(TotalEntries ?? throw new InvalidOperationException($"{nameof(TotalEntries)} is required."));
        writer.Write(CentralDirectorySize ?? throw new InvalidOperationException($"{nameof(CentralDirectorySize)} is required."));
        writer.Write(CentralDirectoryOffset ?? throw new InvalidOperationException($"{nameof(CentralDirectoryOffset)} is required."));
    }

    public override string ToString()
        => $"EOCD64[Entries={TotalEntries}, Size={CentralDirectorySize}, Offset={CentralDirectoryOffset}]";

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
