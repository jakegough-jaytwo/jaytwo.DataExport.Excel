using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;
using Xunit.Abstractions;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32EndOfCentralDirectoryTests
{
    private readonly ITestOutputHelper _output;

    public Zip32EndOfCentralDirectoryTests(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }

    [Fact]
    public void WriteTo_WritesCorrectSignature()
    {
        var bytes = CreateEocdBytes();
        var parsed = ParseZip32EndOfCentralDirectory(bytes);
        Assert.Equal(Zip32EndOfCentralDirectory.KnownSignature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectEntryCounts()
    {
        ushort expected = 42;
        var bytes = CreateEocdBytes(totalEntries: expected);
        var parsed = ParseZip32EndOfCentralDirectory(bytes);

        Assert.Equal(expected, parsed.TotalEntriesOnThisDisk);
        Assert.Equal(expected, parsed.TotalEntries);
    }

    [Fact]
    public void WriteTo_WritesCorrectDirectorySizeAndOffset()
    {
        uint size = 1024;
        uint offset = 4096;
        var bytes = CreateEocdBytes(size: size, offset: offset);
        var parsed = ParseZip32EndOfCentralDirectory(bytes);

        Assert.Equal(size, parsed.CentralDirectorySize);
        Assert.Equal(offset, parsed.CentralDirectoryOffset);
    }

    [Fact]
    public void WriteTo_WritesCommentCorrectly()
    {
        var comment = "Test ZIP comment";
        var bytes = CreateEocdBytes(comment: comment);
        var parsed = ParseZip32EndOfCentralDirectory(bytes);

        Assert.Equal((ushort)comment.Length, parsed.CommentLength);
        Assert.Equal(comment, parsed.Comment);
    }

    [Fact]
    public void WriteTo_WritesEmptyCommentIfNull()
    {
        var bytes = CreateEocdBytes(comment: null);
        var parsed = ParseZip32EndOfCentralDirectory(bytes);

        Assert.Equal(0, parsed.CommentLength!.Value);
        Assert.Equal(string.Empty, parsed.Comment);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var eocd = CreateEocd();
        Assert.Throws<ArgumentException>(() => eocd.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var eocd = CreateEocd();
        var readOnly = new MemoryStream(new byte[64], writable: false);
        Assert.Throws<ArgumentException>(() => eocd.WriteTo(readOnly));
    }

    private static Zip32EndOfCentralDirectory CreateEocd(
        ushort totalEntries = 1,
        uint size = 100,
        uint offset = 200,
        string? comment = "default comment")
        => Zip32EndOfCentralDirectory.CreateDefault(
            totalEntries: totalEntries,
            centralDirectorySize: size,
            centralDirectoryOffset: offset,
            comment: comment);

    private static Zip32EndOfCentralDirectory ParseZip32EndOfCentralDirectory(byte[] bytes)
        => Zip32EndOfCentralDirectory.Parse(bytes);

    private byte[] CreateEocdBytes(
        ushort totalEntries = 1,
        uint size = 100,
        uint offset = 200,
        string? comment = "default comment")
    {
        var eocd = CreateEocd(totalEntries, size, offset, comment);
        using var ms = new MemoryStream();
        eocd.WriteTo(ms);
        var bytes = ms.ToArray();

        // Bonus check: ensure minimum EOCD record size
        Assert.True(bytes.Length >= 22, "EOCD record must be at least 22 bytes");

        _output.WriteLine($"Generated EOCD length: {bytes.Length}");
        _output.WriteLine($"Raw Bytes (hex): {BitConverter.ToString(bytes)}");

        return bytes;
    }
}
