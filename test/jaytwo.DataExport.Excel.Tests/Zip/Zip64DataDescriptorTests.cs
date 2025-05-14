using System;
using System.IO;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip64DataDescriptorTests
{
    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        // arrange
        var descriptor = new Zip64DataDescriptor { Signature = value };

        // act
        var bytes = descriptor.GetBytes(validate: false);

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
        var descriptor = new Zip64DataDescriptor { Crc32 = value };

        // act
        var bytes = descriptor.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x12345678UL)]
    [InlineData(0xFFFFFFFFUL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectCompressedSize(ulong value)
    {
        // arrange
        var descriptor = new Zip64DataDescriptor { CompressedSize = value };

        // act
        var bytes = descriptor.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressedSize);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x12345678UL)]
    [InlineData(0xFFFFFFFFUL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectUncompressedSize(ulong value)
    {
        // arrange
        var descriptor = new Zip64DataDescriptor { UncompressedSize = value };

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
        var descriptor = new Zip64DataDescriptor();

        // act & assert
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        // arrange
        var descriptor = new Zip64DataDescriptor();
        using var readOnlyStream = new MemoryStream(new byte[32], writable: false);

        // act & assert
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(readOnlyStream));
    }

    [Fact]
    public void ToString_OutputsExpectedFormat()
    {
        // arrange
        var crc = 12345u;
        var size = 4567u;
        var descriptor = new Zip64DataDescriptor
        {
            Crc32 = crc,
            CompressedSize = size,
        };

        // act
        var text = descriptor.ToString();

        // assert
        Assert.Contains($"CRC={crc}", text);
        Assert.Contains($"Size={size}", text);
    }

    [Theory]
    [InlineData(false, true, true, true, "Signature")]
    [InlineData(true, false, true, true, "Crc32")]
    [InlineData(true, true, false, true, "CompressedSize")]
    [InlineData(true, true, true, false, "UncompressedSize")]
    public void WriteTo_ThrowsIfFieldIsMissing(
        bool hasSig,
        bool hasCrc,
        bool hasCompressed,
        bool hasUncompressed,
        string expectedField)
    {
        // arrange
        var descriptor = new Zip64DataDescriptor
        {
            Signature = hasSig ? Zip64DataDescriptor.KnownSignature : null,
            Crc32 = hasCrc ? 0x12345678 : null,
            CompressedSize = hasCompressed ? 123UL : null,
            UncompressedSize = hasUncompressed ? 456UL : null,
        };

        using var stream = new MemoryStream();

        // act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => descriptor.WriteTo(stream, validate: true));
        Assert.Contains(expectedField, ex.Message);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenInputTooShort()
    {
        // arrange
        var bytes = new byte[10]; // too short for full structure

        // act
        var result = Zip64DataDescriptor.TryParse(bytes, out var parsed);

        // assert
        Assert.False(result);
        Assert.NotNull(parsed);
    }

    [Fact]
    public void Parse_Throws_WhenInputTooShort()
    {
        // arrange
        var bytes = new byte[10];

        // act & assert
        Assert.Throws<ArgumentException>(() => Zip64DataDescriptor.Parse(bytes));
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip64DataDescriptor result)
        => Zip64DataDescriptor.TryParse(bytes, out result);
}
