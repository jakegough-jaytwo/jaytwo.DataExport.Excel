using System;
using System.IO;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64DataDescriptorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        var bytes = new Zip64DataDescriptor() { Signature = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCrc32(uint value)
    {
        var bytes = new Zip64DataDescriptor() { Crc32 = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrectCompressedSize(ulong value)
    {
        var bytes = new Zip64DataDescriptor() { CompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x0123456789ABCDEF)]
    [InlineData(0xFFFFFFFFFFFFFFFF)]
    public void WriteTo_WritesCorrectUncompressedSize(ulong value)
    {
        var bytes = new Zip64DataDescriptor() { UncompressedSize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.UncompressedSize);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var descriptor = new Zip64DataDescriptor { };
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var descriptor = new Zip64DataDescriptor { };
        var readOnlyStream = new MemoryStream(new byte[32], writable: false);
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(readOnlyStream));
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip64DataDescriptor result)
        => Zip64DataDescriptor.TryParse(bytes, out result);
}
