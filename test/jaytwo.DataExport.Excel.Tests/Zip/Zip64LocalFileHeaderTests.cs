using System;
using System.IO;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip64LocalFileHeaderTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        // arrange
        var header = new Zip64LocalFileHeader { Signature = value };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void WriteTo_WritesCorrectFileName_And_FileNameLength(string value)
    {
        // arrange
        var header = new Zip64LocalFileHeader
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
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCrc32(uint value)
    {
        // arrange
        var header = new Zip64LocalFileHeader { Crc32 = value };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCompressedSize(uint value)
    {
        // arrange
        var header = new Zip64LocalFileHeader { CompressedSize = value };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectUncompressedSize(uint value)
    {
        // arrange
        var header = new Zip64LocalFileHeader { UncompressedSize = value };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
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
        var header = new Zip64LocalFileHeader { Zip64CompressedSize = value };

        // act
        var bytes = header.GetBytes(validate: false);

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
        var header = new Zip64LocalFileHeader { Zip64UncompressedSize = value };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Zip64UncompressedSize);
    }

    [Fact]
    public void WriteTo_WritesZip64ExtraFieldCorrectly()
    {
        // arrange
        var header = new Zip64LocalFileHeader { Zip64UncompressedSize = 1 };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(Zip64LocalFileHeader.Zip64ExtraFieldHeaderId, parsed.ParsedZip64HeaderId);
        Assert.Equal((ushort)16, parsed.ParsedZip64DataLength);
    }

    [Fact]
    public void Zip64SizeProperties_WhenOneIsSet_OtherIsPreserved()
    {
        // arrange
        var header = new Zip64LocalFileHeader
        {
            Zip64UncompressedSize = 1111,
        };

        // act
        header.Zip64CompressedSize = 2222;
        var bytes = header.GetBytes(validate: false);
        TryParse(bytes, out var parsed);

        // assert
        Assert.Equal(1111UL, parsed.Zip64UncompressedSize);
        Assert.Equal(2222UL, parsed.Zip64CompressedSize);
    }

    [Fact]
    public void HasValidZip64ExtraField_ReturnsTrueWhenPresent()
    {
        // arrange
        var header = new Zip64LocalFileHeader { Zip64UncompressedSize = 1 };

        // act
        var bytes = header.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.True(parsed.HasValidZip64ExtraField);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        // arrange
        var header = new Zip64LocalFileHeader();

        // act
        var exception = Record.Exception(() => header.WriteTo(null!));

        // assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        // arrange
        var header = new Zip64LocalFileHeader();
        using var stream = new MemoryStream(new byte[64], writable: false);

        // act
        var exception = Record.Exception(() => header.WriteTo(stream));

        // assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void ToString_ContainsFileName_Crc32_AndCompressedSize()
    {
        // arrange
        var fileName = "test.xlsx";
        var crc32 = 12345u;
        var compressedSize = 67890u;

        var header = new Zip64LocalFileHeader
        {
            FileName = fileName,
            Crc32 = crc32,
            Zip64CompressedSize = compressedSize,
        };

        // act
        var str = header.ToString();

        // assert
        Assert.Contains(fileName, str);
        Assert.Contains(crc32.ToString(), str);
        Assert.Contains(compressedSize.ToString(), str);
    }

    [Fact]
    public void HasValidZip64ExtraField_ReturnsFalse_IfExtraFieldIsTooShort()
    {
        // arrange
        var header = new Zip64LocalFileHeader
        {
            ExtraField = new byte[5], // shorter than 20
            ExtraFieldLength = 5,
        };

        // act
        var hasValidZip64ExtraField = header.HasValidZip64ExtraField;

        // assert
        Assert.False(hasValidZip64ExtraField);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenInputIsInvalid()
    {
        // arrange
        var invalidBytes = new byte[5]; // clearly invalid

        // act
        var success = TryParse(invalidBytes, out var parsed);

        // assert
        Assert.False(success);
        Assert.NotNull(parsed);
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip64LocalFileHeader result)
        => Zip64LocalFileHeader.TryParse(bytes, out result);
}
