using System;
using System.IO;
using System.Text;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64LocalFileHeaderTests
{
    [Fact]
    public void WriteTo_WritesCorrectSignature()
    {
        var bytes = CreateHeaderBytes("test.txt");
        var parsed = ParseZip64LocalFileHeader(bytes);
        Assert.Equal(Zip64LocalFileHeader.Signature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectVersionAndFlags()
    {
        var bytes = CreateHeaderBytes("version-check.txt");
        var parsed = ParseZip64LocalFileHeader(bytes);

        Assert.Equal(45, parsed.VersionNeeded); // ZIP64 requires version 4.5
        Assert.Equal(0x08, parsed.Flags); // Data descriptor follows
    }

    [Fact]
    public void WriteTo_WritesZeroPlaceholders()
    {
        var bytes = CreateHeaderBytes("zero.txt");
        var parsed = ParseZip64LocalFileHeader(bytes);

        Assert.Equal(0u, parsed.Crc32);
        Assert.Equal(0xFFFFFFFF, parsed.CompressedSizePlaceholder);
        Assert.Equal(0xFFFFFFFF, parsed.UncompressedSizePlaceholder);
        Assert.Equal(0UL, parsed.Zip64UncompressedSize);
        Assert.Equal(0UL, parsed.Zip64CompressedSize);
    }

    [Fact]
    public void WriteTo_WritesFileNameCorrectly()
    {
        var fileName = "hello.txt";
        var bytes = CreateHeaderBytes(fileName);
        var parsed = ParseZip64LocalFileHeader(bytes);

        Assert.Equal((ushort)fileName.Length, parsed.FileNameLength);
        Assert.Equal(fileName, parsed.FileName);
    }

    [Fact]
    public void WriteTo_WritesExtraFieldWithCorrectHeaderAndSize()
    {
        var bytes = CreateHeaderBytes("zip64-extra-check.txt");
        var parsed = ParseZip64LocalFileHeader(bytes);

        Assert.Equal(0x0001, parsed.Zip64ExtraFieldHeaderId);
        Assert.Equal(16, parsed.Zip64ExtraFieldSize); // 8 + 8 bytes
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var header = new Zip64LocalFileHeader { FileName = "bad.txt" };
        Assert.Throws<ArgumentException>(() => header.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var header = new Zip64LocalFileHeader { FileName = "readonly.txt" };
        var stream = new MemoryStream(new byte[128], writable: false);
        Assert.Throws<ArgumentException>(() => header.WriteTo(stream));
    }

    // --- Helpers ---

    private static byte[] CreateHeaderBytes(string fileName)
    {
        var header = new Zip64LocalFileHeader { FileName = fileName };
        using var ms = new MemoryStream();
        header.WriteTo(ms);
        return ms.ToArray();
    }

    private static (
        uint Signature,
        ushort VersionNeeded,
        ushort Flags,
        ushort CompressionMethod,
        ushort FileNameLength,
        ushort ExtraFieldLength,
        uint Crc32,
        uint CompressedSizePlaceholder,
        uint UncompressedSizePlaceholder,
        string FileName,
        ushort Zip64ExtraFieldHeaderId,
        ushort Zip64ExtraFieldSize,
        ulong Zip64UncompressedSize,
        ulong Zip64CompressedSize)
        ParseZip64LocalFileHeader(byte[] bytes)
    {
        const int FixedHeaderLength = 30;

        uint sig = BitConverter.ToUInt32(bytes, 0);
        ushort version = BitConverter.ToUInt16(bytes, 4);
        ushort flags = BitConverter.ToUInt16(bytes, 6);
        ushort method = BitConverter.ToUInt16(bytes, 8);
        uint crc = BitConverter.ToUInt32(bytes, 14);
        uint compPlaceholder = BitConverter.ToUInt32(bytes, 18);
        uint uncompPlaceholder = BitConverter.ToUInt32(bytes, 22);
        ushort nameLen = BitConverter.ToUInt16(bytes, 26);
        ushort extraLen = BitConverter.ToUInt16(bytes, 28);

        string fileName = Encoding.UTF8.GetString(bytes, FixedHeaderLength, nameLen);

        int extraOffset = FixedHeaderLength + nameLen;
        ushort extraId = BitConverter.ToUInt16(bytes, extraOffset);
        ushort extraSize = BitConverter.ToUInt16(bytes, extraOffset + 2);
        ulong uncompSize = BitConverter.ToUInt64(bytes, extraOffset + 4);
        ulong compSize = BitConverter.ToUInt64(bytes, extraOffset + 12);

        return (
            sig,
            version,
            flags,
            method,
            nameLen,
            extraLen,
            crc,
            compPlaceholder,
            uncompPlaceholder,
            fileName,
            extraId,
            extraSize,
            uncompSize,
            compSize);
    }
}
