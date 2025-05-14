using System;
using System.IO;
using jaytwo.DataExport.Excel.Zip.Zip32;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip32DataDescriptorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        // arrange
        var descriptor = new Zip32DataDescriptor() { Signature = value };

        // act
        var bytes = descriptor.GetBytes(validate: false);

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
        var descriptor = new Zip32DataDescriptor() { Crc32 = value };

        // act
        var bytes = descriptor.GetBytes(validate: false);

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
        var descriptor = new Zip32DataDescriptor() { CompressedSize = value };

        // act
        var bytes = descriptor.GetBytes(validate: false);

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
        var descriptor = new Zip32DataDescriptor() { UncompressedSize = value };

        // act
        var bytes = descriptor.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        // arrange
        var descriptor = new Zip32DataDescriptor
        {
            Crc32 = 0x1234,
            UncompressedSize = 100,
        };

        // act & assert
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        // arrange
        var descriptor = new Zip32DataDescriptor
        {
            Crc32 = 0x1234,
            UncompressedSize = 100,
        };

        using var readOnlyStream = new MemoryStream(new byte[32], writable: false);

        // act & assert
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(readOnlyStream));
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenInputTooShort()
    {
        // arrange
        var shortBytes = new byte[10];

        // act
        var result = Zip32DataDescriptor.TryParse(shortBytes, out var descriptor);

        // assert
        Assert.False(result);
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void Parse_Throws_WhenInputTooShort()
    {
        // arrange
        var shortBytes = new byte[10];

        // act & assert
        Assert.Throws<ArgumentException>(() => Zip32DataDescriptor.Parse(shortBytes));
    }

    [Fact]
    public void ToString_FormatsAsExpected()
    {
        // arrange
        uint crc = 1234;
        uint compressedSize = 5678;
        var descriptor = new Zip32DataDescriptor
        {
            Crc32 = crc,
            CompressedSize = compressedSize,
        };

        // act
        var str = descriptor.ToString();

        // assert
        Assert.Contains($"CRC={crc}", str);
        Assert.Contains($"Size={compressedSize}", str);
    }

    [Fact]
    public void WriteTo_ThrowsIfRequiredPropertyNull_WhenValidateTrue()
    {
        // arrange
        var descriptor = new Zip32DataDescriptor
        {
            Signature = null,
            Crc32 = 0x1111,
            CompressedSize = 0x2222,
            UncompressedSize = 0x3333,
        };

        using var stream = new MemoryStream();

        // act & assert
        Assert.Throws<InvalidOperationException>(() => descriptor.WriteTo(stream, validate: true));
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip32DataDescriptor result)
        => Zip32DataDescriptor.TryParse(bytes, out result);
}
