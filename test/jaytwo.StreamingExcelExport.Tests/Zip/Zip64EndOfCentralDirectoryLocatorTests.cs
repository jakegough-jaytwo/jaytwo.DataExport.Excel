using System;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64EndOfCentralDirectoryLocatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        var bytes = new Zip64EndOfCentralDirectoryLocator() { Signature = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrectOffset(ulong value)
    {
        var bytes = new Zip64EndOfCentralDirectoryLocator() { Zip64EndOfCentralDirectoryOffset = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Zip64EndOfCentralDirectoryOffset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCentralDirectoryStartDisk(uint value)
    {
        var bytes = new Zip64EndOfCentralDirectoryLocator() { CentralDirectoryStartDisk = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectoryStartDisk);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCentralTotalDisks(uint value)
    {
        var bytes = new Zip64EndOfCentralDirectoryLocator() { TotalDisks = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.TotalDisks);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var locator = Zip64EndOfCentralDirectoryLocatorFactory.CreateLocator(0);
        Assert.Throws<ArgumentNullException>(() => locator.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_WritesExactly20Bytes()
    {
        var bytes = new Zip64EndOfCentralDirectoryLocator() { }.GetBytes(validate: false);
        Assert.Equal(20, bytes.Length); // 4 + 4 + 8 + 4
    }

    [Fact]
    public void ToString_IncludesOffsetValue()
    {
        ulong offset = 0xDEADBEEFCAFEBABE;
        var locator = Zip64EndOfCentralDirectoryLocatorFactory.CreateLocator(offset);
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

        var ex = Assert.Throws<InvalidOperationException>(() => locator.GetBytes(validate: true));
        Assert.Contains(expectedParam, ex.Message);
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip64EndOfCentralDirectoryLocator result)
        => Zip64EndOfCentralDirectoryLocator.TryParse(bytes, out result);
}
