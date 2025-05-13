using System;
using System.IO;
using jaytwo.StreamingExcelExport.Zip;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32LocalFileHeaderTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        var bytes = new Zip32LocalFileHeader() { Signature = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void WriteTo_WritesCorrectFileNameAndLength(string value)
    {
        var bytes = new Zip32LocalFileHeader() { FileName = value, FileNameLength = (ushort)value.Length }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileName);
        Assert.Equal(value.Length, (int?)parsed.FileNameLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCrc32(uint value)
    {
        var bytes = new Zip32LocalFileHeader() { Crc32 = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCompressedSize(uint value)
    {
        var bytes = new Zip32LocalFileHeader() { CompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectUncompressedSize(uint value)
    {
        var bytes = new Zip32LocalFileHeader() { UncompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var header = new Zip32LocalFileHeader();
        Assert.Throws<ArgumentException>(() => header.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var header = new Zip32LocalFileHeader();
        var stream = new MemoryStream(new byte[64], writable: false);
        Assert.Throws<ArgumentException>(() => header.WriteTo(stream));
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip32LocalFileHeader result)
        => Zip32LocalFileHeader.TryParse(bytes, out result);
}
