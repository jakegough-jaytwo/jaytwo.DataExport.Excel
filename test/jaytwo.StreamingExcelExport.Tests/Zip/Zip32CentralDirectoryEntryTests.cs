using System;
using System.Text;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32CentralDirectoryEntryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        var bytes = new Zip32CentralDirectoryEntry() { Signature = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCrc32(uint value)
    {
        var bytes = new Zip32CentralDirectoryEntry() { Crc32 = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Fact]
    public void WriteTo_WritesFileNameCorrectly()
    {
        var value = "file name";
        var bytes = new Zip32CentralDirectoryEntry() { FileName = value, FileNameLength = (ushort)value.Length }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileName);
    }

    [Fact]
    public void WriteTo_WritesFileCommentCorrectly()
    {
        var value = "zip comment";
        var bytes = new Zip32CentralDirectoryEntry() { FileComment = value, FileCommentLength = (ushort)value.Length }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileComment);
    }

    [Fact]
    public void WriteTo_WritesExtraFieldCorrectly()
    {
        var value = Encoding.UTF8.GetBytes("extra");
        var bytes = new Zip32CentralDirectoryEntry() { ExtraField = value, ExtraFieldLength = (ushort)value.Length }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.ExtraField);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesFileNameLengthCorrectly(ushort value)
    {
        var bytes = new Zip32CentralDirectoryEntry() { FileNameLength = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileNameLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesFileCommentLengthCorrectly(ushort value)
    {
        var bytes = new Zip32CentralDirectoryEntry() { FileCommentLength = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileCommentLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesExtraFieldLengthCorrectly(ushort value)
    {
        var bytes = new Zip32CentralDirectoryEntry() { ExtraFieldLength = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.ExtraFieldLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCompressedSizeCorrectly(uint value)
    {
        var bytes = new Zip32CentralDirectoryEntry() { CompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesUncompressedSizeCorrectly(uint value)
    {
        var bytes = new Zip32CentralDirectoryEntry() { UncompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesLocalHeaderOffsetCorrectly(uint value)
    {
        var bytes = new Zip32CentralDirectoryEntry() { LocalHeaderOffset = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.LocalHeaderOffset);
    }

    [Fact]
    public void WriteTo_ThrowsIfFileNameLengthDoesNotMatchBytes()
    {
        var entry = new Zip32CentralDirectoryEntry() { FileName = "hello", FileNameLength = 999 };
        Assert.Throws<InvalidOperationException>(() => entry.GetBytes(validate: true));
    }

    [Fact]
    public void WriteTo_ThrowsIfFileCommentLengthDoesNotMatchBytes()
    {
        var entry = new Zip32CentralDirectoryEntry() { FileComment = "hello", FileCommentLength = 999 };
        Assert.Throws<InvalidOperationException>(() => entry.GetBytes(validate: true));
    }

    [Fact]
    public void WriteTo_ThrowsIfExtraFieldLengthDoesNotMatchBytes()
    {
        var entry = new Zip32CentralDirectoryEntry() { ExtraField = new byte[] { 1, 2, 3, 4 }, ExtraFieldLength = 999 };
        Assert.Throws<InvalidOperationException>(() => entry.GetBytes(validate: true));
    }

    [Fact]
    public void ToString_OutputsFriendlySummary()
    {
        string fileName = "file.txt";
        uint crc32 = 3405691582;
        uint compressedSize = 1234;
        uint localHeaderOffset = 5678;

        var entry = new Zip32CentralDirectoryEntry()
        {
            FileName = fileName,
            Crc32 = crc32,
            CompressedSize = compressedSize,
            LocalHeaderOffset = localHeaderOffset,
        };

        var text = entry.ToString();

        Assert.Contains(fileName, text);
        Assert.Contains($"{crc32}", text);
        Assert.Contains($"{compressedSize}", text);
        Assert.Contains($"{localHeaderOffset}", text);
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip32CentralDirectoryEntry result)
        => Zip32CentralDirectoryEntry.TryParse(bytes, out result);
}
