using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64DataDescriptorTests
{
    [Fact]
    public void WriteTo_WritesCorrectSignature()
    {
        var bytes = CreateDescriptorBytes();
        var parsed = ParseZip64DataDescriptor(bytes);
        Assert.Equal(Zip64DataDescriptor.KnownSignature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectCrc32()
    {
        uint crc = 0xB16B00B5;
        var bytes = CreateDescriptorBytes(crc: crc);
        var parsed = ParseZip64DataDescriptor(bytes);

        Assert.Equal(crc, parsed.Crc32);
    }

    [Fact]
    public void WriteTo_WritesCorrectCompressedSize()
    {
        ulong size = 9876543210;
        var bytes = CreateDescriptorBytes(compressedSize: size);
        var parsed = ParseZip64DataDescriptor(bytes);

        Assert.Equal(size, parsed.CompressedSize);
    }

    [Fact]
    public void WriteTo_WritesCorrectUncompressedSize()
    {
        ulong size = 9876543210;
        var bytes = CreateDescriptorBytes(uncompressedSize: size);
        var parsed = ParseZip64DataDescriptor(bytes);

        Assert.Equal(size, parsed.UncompressedSize);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var descriptor = CreateDescriptor();
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var descriptor = CreateDescriptor();
        var readOnlyStream = new MemoryStream(new byte[32], writable: false);
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(readOnlyStream));
    }

    private static byte[] CreateDescriptorBytes(
        uint crc = 123,
        ulong compressedSize = 456,
        ulong uncompressedSize = 789)
    {
        var entry = CreateDescriptor(crc, compressedSize, uncompressedSize);
        using var ms = new MemoryStream();
        entry.WriteTo(ms);
        return ms.ToArray();
    }

    private static Zip64DataDescriptor CreateDescriptor(
        uint crc = 123,
        ulong compressedSize = 456,
        ulong uncompressedSize = 789)
        => Zip64DataDescriptor.CreateDefault(crc, compressedSize, uncompressedSize);

    private static Zip64DataDescriptor ParseZip64DataDescriptor(byte[] bytes)
        => Zip64DataDescriptor.Parse(bytes);
}
