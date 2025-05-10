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
        Assert.Equal(Zip64EndOfCentralDirectoryLocator.KnownSignature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectOffset()
    {
        ulong expectedOffset = 1234567890;
        var bytes = CreateLocatorBytes(expectedOffset);
        var parsed = ParseZip64EocdLocator(bytes);
        Assert.Equal(expectedOffset, parsed.Zip64EndOfCentralDirectoryOffset);
    }

    [Fact]
    public void WriteTo_WritesStartDiskAsZero()
    {
        var bytes = CreateLocatorBytes();
        var parsed = ParseZip64EocdLocator(bytes);
        Assert.Equal(0u, parsed.CentralDirectoryStartDisk);
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
        var locator = Zip64EndOfCentralDirectoryLocator.CreateDefault(0);
        Assert.Throws<ArgumentNullException>(() => locator.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_WritesExactly20Bytes()
    {
        var bytes = CreateLocatorBytes();
        Assert.Equal(20, bytes.Length); // 4 + 4 + 8 + 4
    }

    [Fact]
    public void ToString_IncludesOffsetValue()
    {
        ulong offset = 0xDEADBEEFCAFEBABE;
        var locator = Zip64EndOfCentralDirectoryLocator.CreateDefault(offset);
        var result = locator.ToString();
        Assert.Contains(offset.ToString(), result);
    }

    [Theory]
    [InlineData(false, true, true, true, "Signature")]
    [InlineData(true, false, true, true, "CentralDirectoryStartDisk")]
    [InlineData(true, true, false, true, "Zip64EndOfCentralDirectoryOffset")]
    [InlineData(true, true, true, false, "TotalDisks")]
    public void WriteTo_ThrowsIfRequiredFieldIsMissing(bool hasSig, bool hasStartDisk, bool hasOffset, bool hasTotalDisks, string expectedParam)
    {
        var locator = new Zip64EndOfCentralDirectoryLocator
        {
            Signature = hasSig ? Zip64EndOfCentralDirectoryLocator.KnownSignature : null,
            CentralDirectoryStartDisk = hasStartDisk ? 0u : null,
            Zip64EndOfCentralDirectoryOffset = hasOffset ? 98765UL : null,
            TotalDisks = hasTotalDisks ? 1u : null,
        };

        using var ms = new MemoryStream();
        var ex = Assert.Throws<InvalidOperationException>(() => locator.WriteTo(ms));
        Assert.Contains(expectedParam, ex.Message);
    }

    // --- Helpers ---

    private static byte[] CreateLocatorBytes(ulong offset = 987654321)
    {
        var locator = Zip64EndOfCentralDirectoryLocator.CreateDefault(offset);
        using var ms = new MemoryStream();
        locator.WriteTo(ms);
        return ms.ToArray();
    }

    private static Zip64EndOfCentralDirectoryLocator ParseZip64EocdLocator(byte[] bytes)
        => Zip64EndOfCentralDirectoryLocator.Parse(bytes);
}
