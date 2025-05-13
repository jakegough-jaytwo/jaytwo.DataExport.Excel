using System;
using System.IO;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32DataDescriptorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        var bytes = new Zip32DataDescriptor() { Signature = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCrc32(uint value)
    {
        var bytes = new Zip32DataDescriptor() { Crc32 = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCompressedSize(uint value)
    {
        var bytes = new Zip32DataDescriptor() { CompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectUncompressedSize(uint value)
    {
        var bytes = new Zip32DataDescriptor() { UncompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var descriptor = new Zip32DataDescriptor { Crc32 = 0x1234, UncompressedSize = 100 };
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var descriptor = new Zip32DataDescriptor { Crc32 = 0x1234, UncompressedSize = 100 };
        var readOnlyStream = new MemoryStream(new byte[32], writable: false);
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(readOnlyStream));
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip32DataDescriptor result)
        => Zip32DataDescriptor.TryParse(bytes, out result);
}
