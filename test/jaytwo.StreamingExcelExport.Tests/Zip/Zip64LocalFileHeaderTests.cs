using System;
using System.IO;
using jaytwo.StreamingExcelExport.Zip;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64LocalFileHeaderTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        var bytes = new Zip64LocalFileHeader() { Signature = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void WriteTo_WritesCorrectFileNameAndLength(string value)
    {
        var bytes = new Zip64LocalFileHeader() { FileName = value, FileNameLength = (ushort)value.Length }.GetBytes(validate: false);
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
        var bytes = new Zip64LocalFileHeader() { Crc32 = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCompressedSize(uint value)
    {
        var bytes = new Zip64LocalFileHeader() { CompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectUncompressedSize(uint value)
    {
        var bytes = new Zip64LocalFileHeader() { UncompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrectZip64CompressedSize(ulong value)
    {
        var bytes = new Zip64LocalFileHeader() { Zip64CompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Zip64CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrectZip64UncompressedSize(ulong value)
    {
        var bytes = new Zip64LocalFileHeader() { Zip64UncompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Zip64UncompressedSize);
    }

    [Fact]
    public void WriteTo_WritesZip64ExtraFieldCorrectly()
    {
        var bytes = new Zip64LocalFileHeader() { Zip64UncompressedSize = 1 }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);

        Assert.Equal(Zip64LocalFileHeader.Zip64ExtraFieldHeaderId, parsed.ParsedZip64HeaderId);
        Assert.Equal((ushort)16, parsed.ParsedZip64DataLength);
    }

    [Fact]
    public void HasValidZip64ExtraField_ReturnsTrueWhenPresent()
    {
        var bytes = new Zip64LocalFileHeader() { Zip64UncompressedSize = 1 }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.True(parsed.HasValidZip64ExtraField);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var header = new Zip64LocalFileHeader();
        Assert.Throws<ArgumentException>(() => header.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var header = new Zip64LocalFileHeader();
        var stream = new MemoryStream(new byte[64], writable: false);
        Assert.Throws<ArgumentException>(() => header.WriteTo(stream));
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip64LocalFileHeader result)
        => Zip64LocalFileHeader.TryParse(bytes, out result);
}
