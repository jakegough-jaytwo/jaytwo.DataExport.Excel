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
        var bytes = CreateTestDataDescriptorBytes();
        var parsed = ParseZip64DataDescriptor(bytes);
        Assert.Equal(Zip64DataDescriptor.Signature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectCrc32()
    {
        uint crc = 0xB16B00B5;
        var bytes = CreateTestDataDescriptorBytes(crc: crc);
        var parsed = ParseZip64DataDescriptor(bytes);

        Assert.Equal(crc, parsed.Crc32);
    }

    [Fact]
    public void WriteTo_WritesCorrectCompressedSize()
    {
        ulong size = 9876543210;
        var bytes = CreateTestDataDescriptorBytes(compressedSize: size);
        var parsed = ParseZip64DataDescriptor(bytes);

        Assert.Equal(size, parsed.CompressedSize);
    }

    [Fact]
    public void WriteTo_WritesCorrectUncompressedSize()
    {
        ulong size = 9876543210;
        var bytes = CreateTestDataDescriptorBytes(uncompressedSize: size);
        var parsed = ParseZip64DataDescriptor(bytes);

        Assert.Equal(size, parsed.UncompressedSize);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var descriptor = CreateTestDataDescriptor();
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var descriptor = CreateTestDataDescriptor();
        var readOnlyStream = new MemoryStream(new byte[32], writable: false);
        Assert.Throws<ArgumentException>(() => descriptor.WriteTo(readOnlyStream));
    }

    private static byte[] CreateTestDataDescriptorBytes(
        uint crc = 123,
        ulong compressedSize = 456,
        ulong uncompressedSize = 789)
    {
        var entry = CreateTestDataDescriptor(crc, compressedSize, uncompressedSize);
        using var ms = new MemoryStream();
        entry.WriteTo(ms);
        return ms.ToArray();
    }

    private static Zip64DataDescriptor CreateTestDataDescriptor(
        uint crc = 123,
        ulong compressedSize = 456,
        ulong uncompressedSize = 789)
    {
        return new Zip64DataDescriptor
        {
            Crc32 = crc,
            CompressedSize = compressedSize,
            UncompressedSize = uncompressedSize,
        };
    }

    private static (uint Signature, uint Crc32, ulong CompressedSize, ulong UncompressedSize)
        ParseZip64DataDescriptor(byte[] bytes)
    {
        const int signatureOffset = 0;
        const int signatureLength = 4;
        const int crcOffset = signatureOffset + signatureLength;
        const int crcLength = 4;
        const int compressedSizeOffset = crcOffset + crcLength;
        const int compressedSizeLength = 8;
        const int uncompressedSizeOffset = compressedSizeOffset + compressedSizeLength;
        //const int uncompressedSizeLength = 8;

        uint signature = BitConverter.ToUInt32(bytes, 0);
        uint crc32 = BitConverter.ToUInt32(bytes, crcOffset);
        ulong compressed = BitConverter.ToUInt64(bytes, compressedSizeOffset);
        ulong uncompressed = BitConverter.ToUInt64(bytes, uncompressedSizeOffset);

        return (signature, crc32, compressed, uncompressed);
    }
}
