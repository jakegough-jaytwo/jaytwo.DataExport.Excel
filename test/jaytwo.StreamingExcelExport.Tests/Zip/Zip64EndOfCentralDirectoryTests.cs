using System;
using System.IO;
using jaytwo.StreamingExcelExport.Zip;
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
        Assert.Equal(Zip64EndOfCentralDirectory.KnownSignature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectVersion()
    {
        var bytes = CreateEocd64Bytes();
        var parsed = ParseZip64EndOfCentralDirectory(bytes);

        Assert.Equal(ZipConstants.Versions.Version45, parsed.VersionMadeBy!.Value);
        Assert.Equal(ZipConstants.Versions.Version45, parsed.VersionNeededToExtract!.Value);
    }

    [Fact]
    public void WriteTo_WritesCorrectEntryCounts()
    {
        ulong total = 12345;
        var bytes = CreateEocd64Bytes(totalEntries: total);
        var parsed = ParseZip64EndOfCentralDirectory(bytes);

        Assert.Equal(total, parsed.TotalEntriesOnThisDisk);
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
        var eocd = Zip64EndOfCentralDirectory.CreateDefault(
            totalEntries: totalEntries,
            centralDirectorySize: size,
            centralDirectoryOffset: offset);

        using var ms = new MemoryStream();
        eocd.WriteTo(ms);
        return ms.ToArray();
    }

    private static Zip64EndOfCentralDirectory ParseZip64EndOfCentralDirectory(byte[] bytes)
        => Zip64EndOfCentralDirectory.Parse(bytes);
}
