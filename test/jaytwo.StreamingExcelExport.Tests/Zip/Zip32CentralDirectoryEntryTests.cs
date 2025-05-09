using System;
using System.IO;
using System.Text;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32CentralDirectoryEntryTests
{
    [Fact]
    public void WriteTo_WritesCorrectSignature()
    {
        var bytes = CreateTestEntryBytes();
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal(Zip32CentralDirectoryEntry.KnownSignature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectCrc32()
    {
        uint crc = 0x12345678;
        var bytes = CreateTestEntryBytes(crc: crc);
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal(crc, parsed.Crc32);
    }

    [Fact]
    public void WriteTo_WritesFileNameCorrectly()
    {
        var fileName = "hello.txt";
        var bytes = CreateTestEntryBytes(fileName: fileName);
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal(fileName, parsed.FileName);
    }

    [Fact]
    public void WriteTo_WritesFileCommentCorrectly()
    {
        var comment = "zip comment";
        var bytes = CreateTestEntryBytes(comment: comment);
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal(comment, parsed.FileComment);
    }

    [Fact]
    public void WriteTo_WritesExtraFieldCorrectly()
    {
        var extraField = Encoding.UTF8.GetBytes("extra!extra!");
        var bytes = CreateTestEntryBytes(extraField: extraField);
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal(extraField, parsed.ExtraField);
    }

    [Fact]
    public void WriteTo_WritesFileNameLengthCorrectly()
    {
        var fileName = "test.txt";
        var bytes = CreateTestEntryBytes(fileName: fileName);
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal((ushort)Encoding.UTF8.GetByteCount(fileName), parsed.FileNameLength);
    }

    [Fact]
    public void WriteTo_WritesFileCommentLengthCorrectly()
    {
        var comment = "comment!";
        var bytes = CreateTestEntryBytes(comment: comment);
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal((ushort)Encoding.UTF8.GetByteCount(comment), parsed.FileCommentLength);
    }

    [Fact]
    public void WriteTo_WritesExtraFieldLengthCorrectly()
    {
        var extraField = Encoding.UTF8.GetBytes("extra!extra!");
        var bytes = CreateTestEntryBytes(extraField: extraField);
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal((ushort)extraField.Length, parsed.ExtraFieldLength);
    }

    [Fact]
    public void WriteTo_WritesCompressedSizeCorrectly()
    {
        uint size = 1024;
        var bytes = CreateTestEntryBytes(compressedSize: size);
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal(size, parsed.CompressedSize);
    }

    [Fact]
    public void WriteTo_WritesUncompressedSizeCorrectly()
    {
        uint size = 1024;
        var bytes = CreateTestEntryBytes(uncompressedSize: size);
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal(size, parsed.UncompressedSize);
    }

    [Fact]
    public void WriteTo_WritesLocalHeaderOffsetCorrectly()
    {
        uint offset = 987654321;
        var bytes = CreateTestEntryBytes(localHeaderOffset: offset);
        var parsed = ParseZip32CentralDirectoryEntry(bytes);
        Assert.Equal(offset, parsed.LocalHeaderOffset);
    }

    [Fact]
    public void WriteTo_ThrowsIfFileNameLengthDoesNotMatchBytes()
    {
        var entry = Zip32CentralDirectoryEntry.CreateDefault(fileName: "abc");
        entry.FileNameLength = 999; // intentionally wrong
        using var ms = new MemoryStream();
        Assert.Throws<ArgumentException>(() => entry.WriteTo(ms));
    }

    [Fact]
    public void WriteTo_ThrowsIfFileCommentLengthDoesNotMatchBytes()
    {
        var entry = CreateTestEntry(fileComment: "xyz");
        entry.FileCommentLength = 1; // intentionally wrong
        using var ms = new MemoryStream();
        Assert.Throws<ArgumentException>(() => entry.WriteTo(ms));
    }

    [Fact]
    public void ToString_OutputsFriendlySummary()
    {
        string fileName = "file.txt";
        uint crc32 = 3405691582;
        uint compressedSize = 1234;
        uint localHeaderOffset = 5678;
        var entry = Zip32CentralDirectoryEntry.CreateDefault(
            fileName: fileName, crc32: crc32, compressedSize: compressedSize, localHeaderOffset: localHeaderOffset);

        var text = entry.ToString();

        Assert.Contains(fileName, text);
        Assert.Contains($"{crc32}", text);
        Assert.Contains($"{compressedSize}", text);
        Assert.Contains($"{localHeaderOffset}", text);
    }

    // --- Helpers ---

    private static Zip32CentralDirectoryEntry CreateTestEntry(
        uint crc32 = 0,
        string fileName = "file.txt",
        string? fileComment = "comment",
        uint? compressedSize = 123,
        uint? uncompressedSize = 456,
        uint? localHeaderOffset = 789,
        byte[]? extraField = null)
    {
        var result = Zip32CentralDirectoryEntry.CreateDefault(
            crc32: crc32,
            fileName: fileName,
            fileComment: fileComment,
            compressedSize: compressedSize,
            uncompressedSize: uncompressedSize,
            localHeaderOffset: localHeaderOffset);

        result.ExtraField = extraField;
        result.ExtraFieldLength = (ushort)(extraField?.Length ?? 0);

        return result;
    }

    private static byte[] CreateTestEntryBytes(
        uint crc = 0,
        string fileName = "file.txt",
        string? comment = "comment",
        uint? compressedSize = 123,
        uint? uncompressedSize = 456,
        uint? localHeaderOffset = 789,
        byte[]? extraField = null)
    {
        var entry = CreateTestEntry(
            crc32: crc,
            fileName: fileName,
            fileComment: comment,
            compressedSize: compressedSize,
            uncompressedSize: uncompressedSize,
            localHeaderOffset: localHeaderOffset,
            extraField: extraField);

        using var ms = new MemoryStream();
        entry.WriteTo(ms);
        return ms.ToArray();
    }

    private static Zip32CentralDirectoryEntry ParseZip32CentralDirectoryEntry(byte[] bytes)
        => Zip32CentralDirectoryEntry.Parse(bytes);
}
