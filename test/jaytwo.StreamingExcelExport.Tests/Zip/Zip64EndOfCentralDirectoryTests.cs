using System;
using System.IO;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64EndOfCentralDirectoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        var bytes = new Zip64EndOfCentralDirectory() { Signature = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrectTotalEntries(ulong value)
    {
        var bytes = new Zip64EndOfCentralDirectory() { TotalEntries = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.TotalEntries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrectTotalEntriesOnThisDisk(ulong value)
    {
        var bytes = new Zip64EndOfCentralDirectory() { TotalEntriesOnThisDisk = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.TotalEntriesOnThisDisk);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrectCentralDirectorySize(ulong value)
    {
        var bytes = new Zip64EndOfCentralDirectory() { CentralDirectorySize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectorySize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrectCentralDirectoryOffset(ulong value)
    {
        var bytes = new Zip64EndOfCentralDirectory() { CentralDirectoryOffset = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectoryOffset);
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
        var readOnly = new MemoryStream(new byte[64], writable: false);
        Assert.Throws<ArgumentException>(() => eocd.WriteTo(readOnly));
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip64EndOfCentralDirectory result)
        => Zip64EndOfCentralDirectory.TryParse(bytes, out result);
}
