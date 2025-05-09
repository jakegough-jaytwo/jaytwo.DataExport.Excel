using System;
using System.IO;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64EndOfCentralDirectoryTests
{
    [Fact]
    public void WriteTo_WritesCorrectSignature()
    {
        var bytes = CreateEocd64Bytes();
        var parsed = ParseZip64EndOfCentralDirectory(bytes);
        Assert.Equal(Zip64EndOfCentralDirectory.Signature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectVersion()
    {
        var bytes = CreateEocd64Bytes();
        var parsed = ParseZip64EndOfCentralDirectory(bytes);

        Assert.Equal(0x2D, parsed.VersionMadeBy); // 0x2D == 45
        Assert.Equal(0x2D, parsed.VersionNeededToExtract);
    }

    [Fact]
    public void WriteTo_WritesCorrectEntryCounts()
    {
        ulong total = 12345;
        var bytes = CreateEocd64Bytes(totalEntries: total);
        var parsed = ParseZip64EndOfCentralDirectory(bytes);

        Assert.Equal(total, parsed.TotalEntriesOnDisk);
        Assert.Equal(total, parsed.TotalEntries);
    }

    [Fact]
    public void WriteTo_WritesCorrectSizeAndOffset()
    {
        ulong size = 65536;
        ulong offset = 999999;
        var bytes = CreateEocd64Bytes(size: size, offset: offset);
        var parsed = ParseZip64EndOfCentralDirectory(bytes);

        Assert.Equal(size, parsed.CentralDirectorySize);
        Assert.Equal(offset, parsed.CentralDirectoryOffset);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var eocd = new Zip64EndOfCentralDirectory();
        Assert.Throws<ArgumentException>(() => eocd.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var eocd = new Zip64EndOfCentralDirectory();
        var readOnly = new MemoryStream(new byte[128], writable: false);
        Assert.Throws<ArgumentException>(() => eocd.WriteTo(readOnly));
    }

    // --- Helpers ---

    private static byte[] CreateEocd64Bytes(ulong totalEntries = 1, ulong size = 100, ulong offset = 200)
    {
        var eocd = new Zip64EndOfCentralDirectory
        {
            TotalEntries = totalEntries,
            CentralDirectorySize = size,
            CentralDirectoryOffset = offset,
        };

        using var ms = new MemoryStream();
        eocd.WriteTo(ms);
        return ms.ToArray();
    }

    private static (
        uint Signature,
        ulong EocdRecordSize,
        ushort VersionMadeBy,
        ushort VersionNeededToExtract,
        uint DiskNumber,
        uint StartDiskNumber,
        ulong TotalEntriesOnDisk,
        ulong TotalEntries,
        ulong CentralDirectorySize,
        ulong CentralDirectoryOffset)
        ParseZip64EndOfCentralDirectory(byte[] bytes)
    {
        uint signature = BitConverter.ToUInt32(bytes, 0);
        ulong size = BitConverter.ToUInt64(bytes, 4);
        ushort versionMade = BitConverter.ToUInt16(bytes, 12);
        ushort versionNeeded = BitConverter.ToUInt16(bytes, 14);
        uint diskNumber = BitConverter.ToUInt32(bytes, 16);
        uint startDisk = BitConverter.ToUInt32(bytes, 20);
        ulong entriesThisDisk = BitConverter.ToUInt64(bytes, 24);
        ulong totalEntries = BitConverter.ToUInt64(bytes, 32);
        ulong cdSize = BitConverter.ToUInt64(bytes, 40);
        ulong cdOffset = BitConverter.ToUInt64(bytes, 48);

        return (
            signature,
            size,
            versionMade,
            versionNeeded,
            diskNumber,
            startDisk,
            entriesThisDisk,
            totalEntries,
            cdSize,
            cdOffset);
    }
}
