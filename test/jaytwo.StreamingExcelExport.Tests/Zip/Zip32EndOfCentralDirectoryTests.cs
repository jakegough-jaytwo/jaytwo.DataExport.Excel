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
        Assert.Equal(Zip32EndOfCentralDirectory.Signature, parsed.Signature);
    }

    [Fact]
    public void WriteTo_WritesCorrectEntryCounts()
    {
        ushort expected = 42;
        var bytes = CreateEocdBytes(totalEntries: expected);
        var parsed = ParseZip32EndOfCentralDirectory(bytes);

        Assert.Equal(expected, parsed.TotalEntriesOnDisk);
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

        Assert.Equal(0, parsed.CommentLength);
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
    {
        return new Zip32EndOfCentralDirectory
        {
            TotalEntries = totalEntries,
            CentralDirectorySize = size,
            CentralDirectoryOffset = offset,
            Comment = comment,
        };
    }

    private static (
        uint Signature,
        ushort TotalEntriesOnDisk,
        ushort TotalEntries,
        uint CentralDirectorySize,
        uint CentralDirectoryOffset,
        ushort CommentLength,
        string Comment)
        ParseZip32EndOfCentralDirectory(byte[] bytes)
    {
        if (bytes.Length < 22)
        {
            throw new InvalidOperationException("EOCD record is too short.");
        }

        const int signatureOffset = 0;
        const int signatureLength = 4;
        const int diskNumberOffset = signatureOffset + signatureLength;
        const int diskNumberLength = 2;
        const int startDiskOffset = diskNumberOffset + diskNumberLength;
        const int startDiskLength = 2;
        const int entriesOnDiskOffset = startDiskOffset + startDiskLength;
        const int entriesOnDiskLength = 2;
        const int totalEntriesOffset = entriesOnDiskOffset + entriesOnDiskLength;
        const int totalEntriesLength = 2;
        const int centralDirectorySizeOffset = totalEntriesOffset + totalEntriesLength;
        const int centralDirectorySizeLength = 4;
        const int centralDirectoryOfffsetOffset = centralDirectorySizeOffset + centralDirectorySizeLength;
        const int centralDirectoryOfffsetLength = 4;
        const int commentLengthOffset = centralDirectoryOfffsetOffset + centralDirectoryOfffsetLength;
        const int commentLengthLength = 2;
        const int commmentOffset = commentLengthOffset + commentLengthLength;

        uint signature = BitConverter.ToUInt32(bytes, signatureOffset);
        ushort entriesOnDisk = BitConverter.ToUInt16(bytes, entriesOnDiskOffset);
        ushort totalEntries = BitConverter.ToUInt16(bytes, totalEntriesOffset);
        var size = BitConverter.ToUInt32(bytes, centralDirectorySizeOffset);
        var offset = BitConverter.ToUInt32(bytes, centralDirectoryOfffsetOffset);
        var commentLength = BitConverter.ToUInt16(bytes, commentLengthOffset);
        string comment = Encoding.UTF8.GetString(bytes, commmentOffset, commentLength);

        return (signature, entriesOnDisk, totalEntries, size, offset, commentLength, comment);
    }

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
