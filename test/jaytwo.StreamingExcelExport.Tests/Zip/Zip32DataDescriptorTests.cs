using System;
using System.IO;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32DataDescriptorTests
{
    [Fact]
    public void WriteTo_WritesCorrectSignature()
    {
        var bytes = CreateDescriptorBytes();
        var parsed = ParseDescriptor(bytes);
        Assert.Equal(Zip32DataDescriptor.KnownSignature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectCrc32()
    {
        uint crc = 0xDEADBEEF;
        var bytes = CreateDescriptorBytes(crc: crc);
        var parsed = ParseDescriptor(bytes);
        Assert.Equal(crc, parsed.Crc32);
    }

    [Fact]
    public void WriteTo_WritesCorrectCompressedSize()
    {
        uint size = 123456;
        var bytes = CreateDescriptorBytes(compressedSize: size);
        var parsed = ParseDescriptor(bytes);

        Assert.Equal(size, parsed.CompressedSize);
    }

    [Fact]
    public void WriteTo_WritesCorrectUncompressedSize()
    {
        uint size = 123456;
        var bytes = CreateDescriptorBytes(uncompressedSize: size);
        var parsed = ParseDescriptor(bytes);

        Assert.Equal(size, parsed.UncompressedSize);
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

    private static byte[] CreateDescriptorBytes(
        uint crc = 123,
        uint compressedSize = 456,
        uint uncompressedSize = 789)
    {
        var entry = CreateDescriptor(crc, compressedSize, uncompressedSize);
        using var ms = new MemoryStream();
        entry.WriteTo(ms);
        return ms.ToArray();
    }

    private static Zip32DataDescriptor CreateDescriptor(
        uint crc = 123,
        uint compressedSize = 456,
        uint uncompressedSize = 789)
        => Zip32DataDescriptor.CreateDefault(crc, compressedSize, uncompressedSize);

    private static Zip32DataDescriptor ParseDescriptor(byte[] bytes)
        => Zip32DataDescriptor.Parse(bytes);
}
