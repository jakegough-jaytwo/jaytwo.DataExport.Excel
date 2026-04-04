using System;
using System.IO;
using jaytwo.DataExport.Excel.Zip.Zip32;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip32LocalFileHeaderTests
{
    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        // arrange
        var header = new Zip32LocalFileHeader { Signature = value };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void WriteTo_WritesCorrectFileNameAndLength(string value)
    {
        // arrange
        var header = new Zip32LocalFileHeader
        {
            FileName = value,
            FileNameLength = (ushort)value.Length,
        };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.FileName);
        Assert.Equal((ushort)value.Length, parsed.FileNameLength);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectCrc32(uint value)
    {
        // arrange
        var header = new Zip32LocalFileHeader { Crc32 = value };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectCompressedSize(uint value)
    {
        // arrange
        var header = new Zip32LocalFileHeader { CompressedSize = value };

        // act
        var bytes = header.GetBytes(validate: false);

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
        var header = new Zip32LocalFileHeader { UncompressedSize = value };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        // arrange
        var header = new Zip32LocalFileHeader();

        // act
        var exception = Record.Exception(() => header.WriteTo(null!));

        // assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        // arrange
        var header = new Zip32LocalFileHeader();
        using var stream = new MemoryStream(new byte[64], writable: false);

        // act
        var exception = Record.Exception(() => header.WriteTo(stream));

        // assert
        Assert.IsType<ArgumentException>(exception);
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip32LocalFileHeader result)
        => Zip32LocalFileHeader.TryParse(bytes, out result);
}
