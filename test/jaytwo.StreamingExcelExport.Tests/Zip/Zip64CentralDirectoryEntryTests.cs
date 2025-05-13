using System;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64CentralDirectoryEntryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        var bytes = new Zip64CentralDirectoryEntry() { Signature = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCrc32(uint value)
    {
        var bytes = new Zip64CentralDirectoryEntry() { Crc32 = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void WriteTo_WritesFileNameCorrectly(string value)
    {
        var bytes = new Zip64CentralDirectoryEntry() { FileName = value, FileNameLength = (ushort)value.Length }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void WriteTo_WritesFileCommentCorrectly(string value)
    {
        var bytes = new Zip64CentralDirectoryEntry() { FileComment = value, FileCommentLength = (ushort)value.Length }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileComment);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCompressedSize(uint value)
    {
        var bytes = new Zip64CentralDirectoryEntry() { CompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectUncompressedSize(uint value)
    {
        var bytes = new Zip64CentralDirectoryEntry() { UncompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrecLocalHeaderOffset(uint value)
    {
        var bytes = new Zip64CentralDirectoryEntry() { LocalHeaderOffset = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.LocalHeaderOffset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrectZip64CompressedSize(ulong value)
    {
        var bytes = new Zip64CentralDirectoryEntry() { Zip64CompressedSize = value }.GetBytes(validate: false);
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
        var bytes = new Zip64CentralDirectoryEntry() { Zip64UncompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Zip64UncompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrecZip64LocalHeaderOffset(ulong value)
    {
        var bytes = new Zip64CentralDirectoryEntry() { Zip64LocalHeaderOffset = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Zip64LocalHeaderOffset);
    }

    [Fact]
    public void WriteTo_WritesZip64ExtraFieldCorrectly()
    {
        var bytes = new Zip64CentralDirectoryEntry() { Zip64LocalHeaderOffset = 1 }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);

        Assert.Equal(Zip64CentralDirectoryEntry.Zip64ExtraFieldHeaderId, parsed.ParsedZip64HeaderId);
        Assert.Equal((ushort)24, parsed.ParsedZip64DataLength);
    }

    [Fact]
    public void HasValidZip64ExtraField_ReturnsTrueWhenPresent()
    {
        var bytes = new Zip64CentralDirectoryEntry() { Zip64LocalHeaderOffset = 1 }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.True(parsed.HasValidZip64ExtraField);
    }

    [Fact]
    public void ToString_OutputsExpectedFormat()
    {
        string fileName = "file.txt";
        uint crc32 = 3405691582;
        uint compressedSize = 1234;
        uint localHeaderOffset = 5678;

        var entry = new Zip64CentralDirectoryEntry()
        {
            FileName = fileName,
            Crc32 = crc32,
            Zip64CompressedSize = compressedSize,
            Zip64LocalHeaderOffset = localHeaderOffset,
        };

        var text = entry.ToString();

        Assert.Contains(fileName, text);
        Assert.Contains($"{crc32}", text);
        Assert.Contains($"{compressedSize}", text);
        Assert.Contains($"{localHeaderOffset}", text);
    }

    [Fact]
    public void WriteTo_ThrowsIfFileNameLengthDoesNotMatchBytes()
    {
        var entry = new Zip64CentralDirectoryEntry() { FileName = "hello", FileNameLength = 999 };
        Assert.Throws<InvalidOperationException>(() => entry.GetBytes(validate: true));
    }

    [Fact]
    public void WriteTo_ThrowsIfFileCommentLengthDoesNotMatchBytes()
    {
        var entry = new Zip64CentralDirectoryEntry() { FileComment = "hello", FileCommentLength = 999 };
        Assert.Throws<InvalidOperationException>(() => entry.GetBytes(validate: true));
    }

    [Fact]
    public void WriteTo_ThrowsIfExtraFieldLengthDoesNotMatchBytes()
    {
        var entry = new Zip64CentralDirectoryEntry() { ExtraField = new byte[] { 1, 2, 3, 4 }, ExtraFieldLength = 999 };
        Assert.Throws<InvalidOperationException>(() => entry.GetBytes(validate: true));
    }

    [Fact]
    public void TryParse_HandlesInvalidExtraFieldGracefully()
    {
        var entry = new Zip64CentralDirectoryEntry();
        entry.ExtraField = new byte[10]; // too short
        entry.ExtraFieldLength = 10;

        var bytes = entry.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.False(parsed.HasValidZip64ExtraField);
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip64CentralDirectoryEntry result)
        => Zip64CentralDirectoryEntry.TryParse(bytes, out result);
}
