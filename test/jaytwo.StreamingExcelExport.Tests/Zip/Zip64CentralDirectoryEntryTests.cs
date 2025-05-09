using System;
using System.IO;
using System.Text;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64CentralDirectoryEntryTests
{
    [Fact]
    public void WriteTo_WritesCorrectSignature()
    {
        var bytes = CreateTestEntryBytes();
        var parsed = ParseZip64CentralDirectoryEntry(bytes);
        Assert.Equal(Zip64CentralDirectoryEntry.KnownSignature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectCrc32()
    {
        uint crc = 0x12345678;
        var bytes = CreateTestEntryBytes(crc: crc);
        var parsed = ParseZip64CentralDirectoryEntry(bytes);
        Assert.Equal(crc, parsed.Crc32);
    }

    [Fact]
    public void WriteTo_WritesFileNameCorrectly()
    {
        var fileName = "hello.txt";
        var bytes = CreateTestEntryBytes(fileName: fileName);
        var parsed = ParseZip64CentralDirectoryEntry(bytes);
        Assert.Equal(fileName, parsed.FileName);
    }

    [Fact]
    public void WriteTo_WritesFileCommentCorrectly()
    {
        var comment = "zip comment";
        var bytes = CreateTestEntryBytes(comment: comment);
        var parsed = ParseZip64CentralDirectoryEntry(bytes);
        Assert.Equal(comment, parsed.FileComment);
    }

    [Fact]
    public void WriteTo_WritesZip64ExtraFieldCorrectly()
    {
        ulong expectedCompressedSize = 1234;
        ulong expectedUncompressedSize = 5678;
        ulong expectedOffset = 987654321;

        var bytes = CreateTestEntryBytes(
            compressedSize: expectedCompressedSize,
            uncompressedSize: expectedUncompressedSize,
            localHeaderOffset: expectedOffset);

        var parsed = ParseZip64CentralDirectoryEntry(bytes);

        Assert.Equal(Zip64CentralDirectoryEntry.Zip64ExtraFieldHeaderId, parsed.ParsedZip64HeaderId);
        Assert.Equal((ushort)24, parsed.ParsedZip64DataLength);
        Assert.Equal(expectedCompressedSize, parsed.Zip64CompressedSize);
        Assert.Equal(expectedUncompressedSize, parsed.Zip64UncompressedSize);
        Assert.Equal(expectedOffset, parsed.Zip64LocalHeaderOffset);
    }

    [Fact]
    public void HasValidZip64ExtraField_ReturnsTrueWhenPresent()
    {
        var bytes = CreateTestEntryBytes();
        var parsed = ParseZip64CentralDirectoryEntry(bytes);
        Assert.True(parsed.HasValidZip64ExtraField);
    }

    [Fact]
    public void ToString_OutputsExpectedFormat()
    {
        var entry = CreateTestEntry(
            fileName: "hello.txt",
            crc: 12345678,
            compressedSize: 789,
            localHeaderOffset: 456);

        var str = entry.ToString();
        Assert.Contains("hello.txt", str);
        Assert.Contains("12345678", str);
        Assert.Contains("789", str);
        Assert.Contains("456", str);
    }

    [Fact]
    public void WriteTo_ThrowsIfFileNameLengthDoesNotMatchBytes()
    {
        var entry = CreateTestEntry(fileName: "abc");
        entry.FileNameLength = 999;
        using var ms = new MemoryStream();
        Assert.Throws<ArgumentException>(() => entry.WriteTo(ms));
    }

    [Fact]
    public void WriteTo_ThrowsIfFileCommentLengthDoesNotMatchBytes()
    {
        var entry = CreateTestEntry(fileName: "abc", comment: "comment");
        entry.FileCommentLength = 1;
        using var ms = new MemoryStream();
        Assert.Throws<ArgumentException>(() => entry.WriteTo(ms));
    }

    [Fact]
    public void WriteTo_ThrowsIfExtraFieldLengthDoesNotMatch()
    {
        var entry = CreateTestEntry(fileName: "abc");
        entry.ExtraField = new byte[] { 1, 2, 3 };
        entry.ExtraFieldLength = 2; // mismatch
        using var ms = new MemoryStream();
        Assert.Throws<ArgumentException>(() => entry.WriteTo(ms));
    }

    [Fact]
    public void Parse_HandlesInvalidExtraFieldGracefully()
    {
        var entry = CreateTestEntry(fileName: "abc");
        entry.ExtraField = new byte[10]; // too short
        entry.ExtraFieldLength = 10;
        using var ms = new MemoryStream();
        entry.WriteTo(ms);

        var parsed = ParseZip64CentralDirectoryEntry(ms.ToArray());
        Assert.False(parsed.HasValidZip64ExtraField);
    }

    // --- Helpers ---

    private static Zip64CentralDirectoryEntry CreateTestEntry(
        uint crc = 0,
        string fileName = "file.txt",
        string? comment = "comment",
        ulong compressedSize = 123,
        ulong uncompressedSize = 456,
        ulong localHeaderOffset = 789)
    => Zip64CentralDirectoryEntry.CreateDefault(
            crc32: crc,
            fileName: fileName,
            fileComment: comment,
            compressedSize: compressedSize,
            uncompressedSize: uncompressedSize,
            localHeaderOffset: localHeaderOffset);

    private static byte[] CreateTestEntryBytes(
        uint crc = 0,
        string fileName = "file.txt",
        string? comment = "comment",
        ulong compressedSize = 123,
        ulong uncompressedSize = 456,
        ulong localHeaderOffset = 789)
    {
        var entry = CreateTestEntry(
            crc: crc,
            fileName: fileName,
            comment: comment,
            compressedSize: compressedSize,
            uncompressedSize: uncompressedSize,
            localHeaderOffset: localHeaderOffset);

        using var ms = new MemoryStream();
        entry.WriteTo(ms);
        return ms.ToArray();
    }

    private static Zip64CentralDirectoryEntry ParseZip64CentralDirectoryEntry(byte[] bytes)
        => Zip64CentralDirectoryEntry.Parse(bytes);
}
