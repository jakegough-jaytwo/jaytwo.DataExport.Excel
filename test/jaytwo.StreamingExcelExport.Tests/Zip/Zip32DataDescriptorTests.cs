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
        var parsed = ParseZip32DataDescriptor(bytes);
        Assert.Equal(Zip32DataDescriptor.Signature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectCrc32()
    {
        uint crc = 0xDEADBEEF;
        var bytes = CreateDescriptorBytes(crc: crc);
        var parsed = ParseZip32DataDescriptor(bytes);
        Assert.Equal(crc, parsed.Crc32);
    }

    [Fact]
    public void WriteTo_WritesCorrectCompressedSize()
    {
        uint size = 123456;
        var bytes = CreateDescriptorBytes(compressedSize: size);
        var parsed = ParseZip32DataDescriptor(bytes);

        Assert.Equal(size, parsed.CompressedSize);
    }

    [Fact]
    public void WriteTo_WritesCorrectUncompressedSize()
    {
        uint size = 123456;
        var bytes = CreateDescriptorBytes(uncompressedSize: size);
        var parsed = ParseZip32DataDescriptor(bytes);

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

    private static byte[] CreateDescriptorBytes(uint crc = 0, uint compressedSize = 0, uint uncompressedSize = 0)
    {
        var descriptor = new Zip32DataDescriptor
        {
            Crc32 = crc,
            CompressedSize = compressedSize,
            UncompressedSize = uncompressedSize,
        };

        using var ms = new MemoryStream();
        descriptor.WriteTo(ms);
        return ms.ToArray();
    }

    private static (uint Signature, uint Crc32, uint CompressedSize, uint UncompressedSize)
        ParseZip32DataDescriptor(byte[] bytes)
    {
        const int signatureOffset = 0;
        const int signatureLength = 4;
        const int crcOffset = signatureOffset + signatureLength;
        const int crcLength = 4;
        const int compressedSizeOffset = crcOffset + crcLength;
        const int compressedSizeLength = 4;
        const int uncompressedSizeOffset = compressedSizeOffset + compressedSizeLength;
        //const int uncompressedSizeLength = 4;

        uint signature = BitConverter.ToUInt32(bytes, 0);
        uint crc32 = BitConverter.ToUInt32(bytes, crcOffset);
        uint compressed = BitConverter.ToUInt32(bytes, compressedSizeOffset);
        uint uncompressed = BitConverter.ToUInt32(bytes, uncompressedSizeOffset);

        return (signature, crc32, compressed, uncompressed);
    }
}
