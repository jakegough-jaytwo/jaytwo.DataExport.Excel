using System;
using System.IO;
using jaytwo.DataExport.Excel.Zip.Zip32;
using Xunit;
using Xunit.Abstractions;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip32EndOfCentralDirectoryTests
{
    private readonly ITestOutputHelper _output;

    public Zip32EndOfCentralDirectoryTests(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory { Signature = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesCorrectTotalEntries(ushort value)
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory { TotalEntries = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.TotalEntries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesCorrectTotalEntriesOnThisDisk(ushort value)
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory { TotalEntriesOnThisDisk = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.TotalEntriesOnThisDisk);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectCentralDirectorySize(uint value)
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory { CentralDirectorySize = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectorySize);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectCentralDirectoryOffset(uint value)
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory { CentralDirectoryOffset = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectoryOffset);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void WriteTo_WritesCorrectComment(string value)
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory
        {
            Comment = value,
            CommentLength = (ushort)value.Length,
        };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Comment);
        Assert.Equal((ushort)value.Length, parsed.CommentLength);
    }

    [Fact]
    public void WriteTo_WritesEmptyCommentIfNull()
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory { Comment = null };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(string.Empty, parsed.Comment);
    }

    [Fact]
    public void WriteTo_ThrowsIfCommentLengthDoesNotMatchActualLength()
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory
        {
            Comment = "hello",
            CommentLength = 999,
        };

        // act
        var exception = Record.Exception(() => eocd.GetBytes(validate: true));

        // assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory();

        // act
        var exception = Record.Exception(() => eocd.WriteTo(null!));

        // assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory();
        using var readOnly = new MemoryStream(new byte[64], writable: false);

        // act
        var exception = Record.Exception(() => eocd.WriteTo(readOnly));

        // assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Theory]
    [InlineData(false, true, true, true, true, true, true, true, "Signature")]
    [InlineData(true, false, true, true, true, true, true, true, "DiskNumber")]
    [InlineData(true, true, false, true, true, true, true, true, "CentralDirectoryStartDisk")]
    [InlineData(true, true, true, false, true, true, true, true, "TotalEntriesOnThisDisk")]
    [InlineData(true, true, true, true, false, true, true, true, "TotalEntries")]
    [InlineData(true, true, true, true, true, false, true, true, "CentralDirectorySize")]
    [InlineData(true, true, true, true, true, true, false, true, "CentralDirectoryOffset")]
    [InlineData(true, true, true, true, true, true, true, false, "CommentLength")]
    public void WriteTo_ThrowsIfFieldMissing(
        bool hasSig,
        bool hasDiskNum,
        bool hasStartDisk,
        bool hasEntriesDisk,
        bool hasEntries,
        bool hasSize,
        bool hasOffset,
        bool hasCommentLength,
        string expectedField)
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory
        {
            Signature = hasSig ? Zip32EndOfCentralDirectory.KnownSignature : null,
            DiskNumber = hasDiskNum ? (ushort)0 : null,
            CentralDirectoryStartDisk = hasStartDisk ? (ushort)0 : null,
            TotalEntriesOnThisDisk = hasEntriesDisk ? (ushort)1 : null,
            TotalEntries = hasEntries ? (ushort)1 : null,
            CentralDirectorySize = hasSize ? 100U : null,
            CentralDirectoryOffset = hasOffset ? 200U : null,
            Comment = string.Empty,
            CommentLength = hasCommentLength ? (ushort)0 : null,
        };

        // act
        var exception = Record.Exception(() => eocd.GetBytes(validate: true));

        // assert
        var ex = Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains(expectedField, ex.Message);
    }

    [Fact]
    public void ToString_OutputsExpectedValues()
    {
        // arrange
        var eocd = new Zip32EndOfCentralDirectory
        {
            TotalEntries = 12,
            CentralDirectorySize = 3456,
            CentralDirectoryOffset = 7890,
        };

        // act
        var text = eocd.ToString();

        // assert
        Assert.Contains("Entries=12", text);
        Assert.Contains("Size=3456", text);
        Assert.Contains("Offset=7890", text);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenInputTooShort()
    {
        // arrange
        var shortBytes = new byte[10];

        // act
        var result = Zip32EndOfCentralDirectory.TryParse(shortBytes, out var parsed);

        // assert
        Assert.False(result);
        Assert.NotNull(parsed);
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip32EndOfCentralDirectory result)
        => Zip32EndOfCentralDirectory.TryParse(bytes, out result);
}
