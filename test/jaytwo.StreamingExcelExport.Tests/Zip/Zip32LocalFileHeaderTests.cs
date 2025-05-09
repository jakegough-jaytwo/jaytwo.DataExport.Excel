using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32LocalFileHeaderTests
{
    [Fact]
    public void WriteTo_WritesCorrectSignature()
    {
        var bytes = CreateHeaderBytes("test.txt");
        var parsed = ParseZip32LocalFileHeader(bytes);
        Assert.Equal(Zip32LocalFileHeader.Signature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectFileNameAndLength()
    {
        var fileName = "hello-world.txt";
        var bytes = CreateHeaderBytes(fileName);
        var parsed = ParseZip32LocalFileHeader(bytes);

        Assert.Equal((ushort)fileName.Length, parsed.FileNameLength);
        Assert.Equal(fileName, parsed.FileName);
    }

    [Fact]
    public void WriteTo_WritesZeroPlaceholders()
    {
        var bytes = CreateHeaderBytes("placeholder.txt");
        var parsed = ParseZip32LocalFileHeader(bytes);

        Assert.Equal(0u, parsed.Crc32);
        Assert.Equal(0u, parsed.CompressedSize);
        Assert.Equal(0u, parsed.UncompressedSize);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var header = new Zip32LocalFileHeader { FileName = "null.txt" };
        Assert.Throws<ArgumentException>(() => header.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var header = new Zip32LocalFileHeader { FileName = "read-only.txt" };
        var stream = new MemoryStream(new byte[64], writable: false);
        Assert.Throws<ArgumentException>(() => header.WriteTo(stream));
    }

    // --- Helpers ---

    private static byte[] CreateHeaderBytes(string fileName)
    {
        var header = new Zip32LocalFileHeader { FileName = fileName };
        using var ms = new MemoryStream();
        header.WriteTo(ms);
        return ms.ToArray();
    }

    private static (
        uint Signature,
        ushort VersionNeeded,
        ushort Flags,
        ushort CompressionMethod,
        ushort LastModTime,
        ushort LastModDate,
        uint Crc32,
        uint CompressedSize,
        uint UncompressedSize,
        ushort FileNameLength,
        ushort ExtraFieldLength,
        string FileName)
        ParseZip32LocalFileHeader(byte[] bytes)
    {
        const int FixedHeaderLength = 30;

        uint signature = BitConverter.ToUInt32(bytes, 0);
        ushort version = BitConverter.ToUInt16(bytes, 4);
        ushort flags = BitConverter.ToUInt16(bytes, 6);
        ushort method = BitConverter.ToUInt16(bytes, 8);
        ushort time = BitConverter.ToUInt16(bytes, 10);
        ushort date = BitConverter.ToUInt16(bytes, 12);
        uint crc = BitConverter.ToUInt32(bytes, 14);
        uint compressed = BitConverter.ToUInt32(bytes, 18);
        uint uncompressed = BitConverter.ToUInt32(bytes, 22);
        ushort nameLen = BitConverter.ToUInt16(bytes, 26);
        ushort extraLen = BitConverter.ToUInt16(bytes, 28);

        string fileName = Encoding.UTF8.GetString(bytes, FixedHeaderLength, nameLen);

        return (
            signature,
            version,
            flags,
            method,
            time,
            date,
            crc,
            compressed,
            uncompressed,
            nameLen,
            extraLen,
            fileName);
    }
}
