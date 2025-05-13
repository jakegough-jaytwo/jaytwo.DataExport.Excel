using System;
using System.IO;
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

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        var bytes = new Zip32EndOfCentralDirectory() { Signature = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesCorrectTotalEntries(ushort value)
    {
        var bytes = new Zip32EndOfCentralDirectory() { TotalEntries = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.TotalEntries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesCorrectTotalEntriesOnThisDisk(ushort value)
    {
        var bytes = new Zip32EndOfCentralDirectory() { TotalEntriesOnThisDisk = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.TotalEntriesOnThisDisk);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCentralDirectorySize(uint value)
    {
        var bytes = new Zip32EndOfCentralDirectory() { CentralDirectorySize = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectorySize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectCentralDirectoryOffset(uint value)
    {
        var bytes = new Zip32EndOfCentralDirectory() { CentralDirectoryOffset = value }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectoryOffset);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void WriteTo_WritesCorrectComment(string value)
    {
        var bytes = new Zip32EndOfCentralDirectory() { Comment = value, CommentLength = (ushort)value.Length }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Comment);
        Assert.Equal(value.Length, (int?)parsed.CommentLength);
    }

    [Fact]
    public void WriteTo_WritesEmptyCommentIfNull()
    {
        var bytes = new Zip32EndOfCentralDirectory() { Comment = null }.GetBytes(validate: false);
        TryParse(bytes, out var parsed);
        Assert.Equal(string.Empty, parsed.Comment);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        var eocd = new Zip32EndOfCentralDirectory();
        Assert.Throws<ArgumentException>(() => eocd.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        var eocd = new Zip32EndOfCentralDirectory();
        var readOnly = new MemoryStream(new byte[64], writable: false);
        Assert.Throws<ArgumentException>(() => eocd.WriteTo(readOnly));
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip32EndOfCentralDirectory result)
        => Zip32EndOfCentralDirectory.TryParse(bytes, out result);
}
