using System;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip64CentralDirectoryEntryTests
{
    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { Signature = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectCrc32(uint value)
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { Crc32 = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void WriteTo_WritesFileNameCorrectly(string value)
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry
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

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void WriteTo_WritesFileCommentCorrectly(string value)
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry
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

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectCompressedSize(uint value)
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { CompressedSize = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressedSize);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectUncompressedSize(uint value)
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { UncompressedSize = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectLocalHeaderOffset(uint value)
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { LocalHeaderOffset = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.LocalHeaderOffset);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x12345678UL)]
    [InlineData(0xFFFFFFFFUL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectZip64CompressedSize(ulong value)
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { Zip64CompressedSize = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Zip64CompressedSize);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x12345678UL)]
    [InlineData(0xFFFFFFFFUL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectZip64UncompressedSize(ulong value)
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { Zip64UncompressedSize = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Zip64UncompressedSize);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x12345678UL)]
    [InlineData(0xFFFFFFFFUL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectZip64LocalHeaderOffset(ulong value)
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { Zip64LocalHeaderOffset = value };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Zip64LocalHeaderOffset);
    }

    [Fact]
    public void WriteTo_WritesZip64ExtraFieldCorrectly()
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { Zip64LocalHeaderOffset = 1 };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(Zip64CentralDirectoryEntry.Zip64ExtraFieldHeaderId, parsed.ParsedZip64HeaderId);
        Assert.Equal((ushort)24, parsed.ParsedZip64DataLength);
    }

    [Fact]
    public void HasValidZip64ExtraField_ReturnsTrueWhenPresent()
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { Zip64LocalHeaderOffset = 1 };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.True(parsed.HasValidZip64ExtraField);
    }

    [Fact]
    public void ToString_OutputsExpectedFormat()
    {
        // arrange
        string fileName = "file.txt";
        uint crc32 = 3405691582;
        uint compressedSize = 1234;
        uint localHeaderOffset = 5678;

        var entry = new Zip64CentralDirectoryEntry
        {
            FileName = fileName,
            Crc32 = crc32,
            Zip64CompressedSize = compressedSize,
            Zip64LocalHeaderOffset = localHeaderOffset,
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
    public void WriteTo_ThrowsIfFileNameLengthDoesNotMatchBytes()
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { FileName = "hello", FileNameLength = 999 };

        // act
        var exception = Record.Exception(() => entry.GetBytes(validate: true));

        // assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void WriteTo_ThrowsIfFileCommentLengthDoesNotMatchBytes()
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { FileComment = "hello", FileCommentLength = 999 };

        // act
        var exception = Record.Exception(() => entry.GetBytes(validate: true));

        // assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void WriteTo_ThrowsIfExtraFieldLengthDoesNotMatchBytes()
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { ExtraField = new byte[] { 1, 2, 3, 4 }, ExtraFieldLength = 999 };

        // act
        var exception = Record.Exception(() => entry.GetBytes(validate: true));

        // assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void TryParse_HandlesInvalidExtraFieldGracefully()
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry { ExtraField = new byte[10], ExtraFieldLength = 10 };

        // act
        var bytes = entry.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.False(parsed.HasValidZip64ExtraField);
    }

    [Fact]
    public void UpdateZip64ExtraField_UpdatesOnlySpecifiedValue()
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry
        {
            Zip64UncompressedSize = 1,
            Zip64CompressedSize = 2,
            Zip64LocalHeaderOffset = 3,
        };

        // act
        entry.Zip64CompressedSize = 42;

        // assert
        Assert.Equal(1ul, entry.Zip64UncompressedSize);
        Assert.Equal(42ul, entry.Zip64CompressedSize);
        Assert.Equal(3ul, entry.Zip64LocalHeaderOffset);
    }

    [Fact]
    public void TryParse_ReturnsFalse_OnMalformedExtraField()
    {
        // arrange
        var entry = new Zip64CentralDirectoryEntry
        {
            ExtraField = new byte[5], // invalid size
            ExtraFieldLength = 5,
        };

        // act
        var bytes = entry.GetBytes(validate: false);
        TryParse(bytes, out var parsed);

        // assert
        Assert.False(parsed.HasValidZip64ExtraField);
        Assert.Null(parsed.Zip64UncompressedSize);
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip64CentralDirectoryEntry result)
        => Zip64CentralDirectoryEntry.TryParse(bytes, out result);
}
