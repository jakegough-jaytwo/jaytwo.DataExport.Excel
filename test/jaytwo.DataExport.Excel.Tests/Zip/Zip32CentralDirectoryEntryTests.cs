using System;
using System.Text;
using jaytwo.DataExport.Excel.Zip.Zip32;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip32CentralDirectoryEntryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { Signature = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCrc32(uint value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { Crc32 = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Fact]
    public void WriteTo_WritesFileNameCorrectly()
    {
        // arrange
        var value = "file name";
        var entry = new Zip32CentralDirectoryEntry()
        {
            FileName = value,
            FileNameLength = (ushort)value.Length,
        };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileName);
    }

    [Fact]
    public void WriteTo_WritesFileCommentCorrectly()
    {
        // arrange
        var value = "zip comment";
        var entry = new Zip32CentralDirectoryEntry()
        {
            FileComment = value,
            FileCommentLength = (ushort)value.Length,
        };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileComment);
    }

    [Fact]
    public void WriteTo_WritesExtraFieldCorrectly()
    {
        // arrange
        var value = Encoding.UTF8.GetBytes("extra");
        var entry = new Zip32CentralDirectoryEntry()
        {
            ExtraField = value,
            ExtraFieldLength = (ushort)value.Length,
        };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.ExtraField);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesFileNameLengthCorrectly(ushort value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { FileNameLength = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileNameLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesFileCommentLengthCorrectly(ushort value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { FileCommentLength = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileCommentLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesExtraFieldLengthCorrectly(ushort value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { ExtraFieldLength = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.ExtraFieldLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCompressedSizeCorrectly(uint value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { CompressedSize = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesUncompressedSizeCorrectly(uint value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { UncompressedSize = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesLocalHeaderOffsetCorrectly(uint value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { LocalHeaderOffset = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.LocalHeaderOffset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesVersionNeededToExtractCorrectly(ushort value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { VersionNeededToExtract = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.VersionNeededToExtract);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesGeneralPurposeBitFlagCorrectly(ushort value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { GeneralPurposeBitFlag = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.GeneralPurposeBitFlag);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesCompressionMethodCorrectly(ushort value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { CompressionMethod = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressionMethod);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesLastModTimeCorrectly(ushort value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { LastModTime = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.LastModTime);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesLastModDateCorrectly(ushort value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { LastModDate = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.LastModDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesDiskNumberStartCorrectly(ushort value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { DiskNumberStart = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.DiskNumberStart);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesInternalFileAttributesCorrectly(ushort value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { InternalFileAttributes = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.InternalFileAttributes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesExternalFileAttributesCorrectly(uint value)
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry() { ExternalFileAttributes = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.ExternalFileAttributes);
    }

    [Fact]
    public void WriteTo_ThrowsIfFileNameLengthDoesNotMatchBytes()
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry()
        {
            FileName = "hello",
            FileNameLength = 999,
        };

        // act & assert
        Assert.Throws<InvalidOperationException>(() => entry.GetBytes(validate: true));
    }

    [Fact]
    public void WriteTo_ThrowsIfFileCommentLengthDoesNotMatchBytes()
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry()
        {
            FileComment = "hello",
            FileCommentLength = 999,
        };

        // act & assert
        Assert.Throws<InvalidOperationException>(() => entry.GetBytes(validate: true));
    }

    [Fact]
    public void WriteTo_ThrowsIfExtraFieldLengthDoesNotMatchBytes()
    {
        // arrange
        var entry = new Zip32CentralDirectoryEntry()
        {
            ExtraField = new byte[] { 1, 2, 3, 4 },
            ExtraFieldLength = 999,
        };

        // act & assert
        Assert.Throws<InvalidOperationException>(() => entry.GetBytes(validate: true));
    }

    [Fact]
    public void ToString_OutputsFriendlySummary()
    {
        // arrange
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

        // act
        var text = entry.ToString();

        // assert
        Assert.Contains(fileName, text);
        Assert.Contains($"{crc32}", text);
        Assert.Contains($"{compressedSize}", text);
        Assert.Contains($"{localHeaderOffset}", text);
    }

    [Fact]
    public void TryParse_ReturnsFalse_ForIncompleteBuffer()
    {
        // arrange
        var incomplete = new byte[5]; // Too small for even the header

        // act
        var success = TryParse(incomplete, out var _);

        // assert
        Assert.False(success);
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip32CentralDirectoryEntry result)
        => Zip32CentralDirectoryEntry.TryParse(bytes, out result);
}
