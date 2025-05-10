using System;
using System.IO;
using System.Text;
using jaytwo.StreamingExcelExport.Zip;
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
        Assert.Equal(Zip64LocalFileHeader.KnownSignature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectVersionAndFlags()
    {
        var bytes = CreateHeaderBytes("version-check.txt");
        var parsed = ParseZip64LocalFileHeader(bytes);

        Assert.Equal(45u, parsed.VersionNeededToExtract!.Value); // ZIP64 requires version 4.5
        Assert.Equal(0x08, parsed.GeneralPurposeBitFlag!.Value); // Data descriptor follows
    }

    [Fact]
    public void WriteTo_WritesZeroPlaceholders()
    {
        var bytes = CreateHeaderBytes("zero.txt");
        var parsed = ParseZip64LocalFileHeader(bytes);

        Assert.Equal(0u, parsed.Crc32);
        Assert.Equal(0xFFFFFFFF, parsed.CompressedSize);
        Assert.Equal(0xFFFFFFFF, parsed.UncompressedSize);
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

        Assert.Equal(0x0001, parsed.ParsedZip64HeaderId!.Value);
        Assert.Equal(16, parsed.ParsedZip64DataLength!.Value); // 8 + 8 bytes
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
        var header = Zip64LocalFileHeader.CreateDefault(
            compressionMethod: ZipConstants.CompressionMethods.NoCompression,
            fileName: fileName);

        using var ms = new MemoryStream();
        header.WriteTo(ms);
        return ms.ToArray();
    }

    private static Zip64LocalFileHeader ParseZip64LocalFileHeader(byte[] bytes)
        => Zip64LocalFileHeader.Parse(bytes);
}
