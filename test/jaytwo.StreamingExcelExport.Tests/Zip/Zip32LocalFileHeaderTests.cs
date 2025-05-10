using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaytwo.StreamingExcelExport.Zip;
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
        Assert.Equal(Zip32LocalFileHeader.KnownSignature, parsed.Signature);
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
        var header = new Zip32LocalFileHeader();
        Assert.Throws<ArgumentException>(() => header.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var header = new Zip32LocalFileHeader();
        var stream = new MemoryStream(new byte[64], writable: false);
        Assert.Throws<ArgumentException>(() => header.WriteTo(stream));
    }

    // --- Helpers ---

    private static byte[] CreateHeaderBytes(string fileName)
    {
        var header = Zip32LocalFileHeader.CreateDefault(
            compressionMethod: ZipConstants.CompressionMethods.NoCompression,
            fileName: fileName);

        using var ms = new MemoryStream();
        header.WriteTo(ms);
        return ms.ToArray();
    }

    private static Zip32LocalFileHeader ParseZip32LocalFileHeader(byte[] bytes)
        => Zip32LocalFileHeader.Parse(bytes);
}
