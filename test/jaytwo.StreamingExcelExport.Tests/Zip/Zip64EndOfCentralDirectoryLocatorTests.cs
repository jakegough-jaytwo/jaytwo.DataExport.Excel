using System;
using System.IO;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64EndOfCentralDirectoryLocatorTests
{
    [Fact]
    public void WriteTo_WritesCorrectSignature()
    {
        var bytes = CreateLocatorBytes();
        var parsed = ParseZip64EocdLocator(bytes);
        Assert.Equal(Zip64EndOfCentralDirectoryLocator.Signature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectOffset()
    {
        ulong expectedOffset = 1234567890;
        var bytes = CreateLocatorBytes(expectedOffset);
        var parsed = ParseZip64EocdLocator(bytes);
        Assert.Equal(expectedOffset, parsed.EocdOffset);
    }

    [Fact]
    public void WriteTo_WritesStartDiskAsZero()
    {
        var bytes = CreateLocatorBytes();
        var parsed = ParseZip64EocdLocator(bytes);
        Assert.Equal(0u, parsed.StartDiskNumber);
    }

    [Fact]
    public void WriteTo_WritesTotalDisksAsOne()
    {
        var bytes = CreateLocatorBytes();
        var parsed = ParseZip64EocdLocator(bytes);
        Assert.Equal(1u, parsed.TotalDisks);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var locator = new Zip64EndOfCentralDirectoryLocator { Zip64EndOfCentralDirectoryOffset = 0 };
        Assert.Throws<ArgumentNullException>(() => locator.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_WritesExactly20Bytes()
    {
        var bytes = CreateLocatorBytes();
        Assert.Equal(20, bytes.Length); // 4 + 4 + 8 + 4
    }

    // --- Helpers ---

    private static byte[] CreateLocatorBytes(ulong offset = 987654321)
    {
        var locator = new Zip64EndOfCentralDirectoryLocator
        {
            Zip64EndOfCentralDirectoryOffset = offset,
        };

        using var ms = new MemoryStream();
        locator.WriteTo(ms);
        return ms.ToArray();
    }

    private static (
        uint Signature,
        uint StartDiskNumber,
        ulong EocdOffset,
        uint TotalDisks)
        ParseZip64EocdLocator(byte[] bytes)
    {
        uint sig = BitConverter.ToUInt32(bytes, 0);
        uint disk = BitConverter.ToUInt32(bytes, 4);
        ulong offset = BitConverter.ToUInt64(bytes, 8);
        uint total = BitConverter.ToUInt32(bytes, 16);

        return (sig, disk, offset, total);
    }
}
