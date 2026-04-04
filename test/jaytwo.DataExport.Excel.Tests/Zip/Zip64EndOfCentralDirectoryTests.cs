using System;
using System.IO;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip64EndOfCentralDirectoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory { Signature = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectTotalEntries(ulong value)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory { TotalEntries = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.TotalEntries);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectTotalEntriesOnThisDisk(ulong value)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory { TotalEntriesOnThisDisk = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.TotalEntriesOnThisDisk);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectCentralDirectorySize(ulong value)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory { CentralDirectorySize = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectorySize);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectCentralDirectoryOffset(ulong value)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory { CentralDirectoryOffset = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectoryOffset);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectSizeOfEOCD(ulong value)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory { SizeOfEOCD = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.SizeOfEOCD);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesCorrectVersionMadeBy(ushort value)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory { VersionMadeBy = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.VersionMadeBy);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void WriteTo_WritesCorrectVersionNeededToExtract(ushort value)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory { VersionNeededToExtract = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.VersionNeededToExtract);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectDiskNumber(uint value)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory { DiskNumber = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.DiskNumber);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectCentralDirectoryStartDisk(uint value)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory { CentralDirectoryStartDisk = value };

        // act
        var bytes = eocd.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectoryStartDisk);
    }

    [Fact]
    public void ToString_OutputsExpectedFormat()
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory
        {
            TotalEntries = 100,
            CentralDirectorySize = 2048,
            CentralDirectoryOffset = 4096,
        };

        // act
        var str = eocd.ToString();

        // assert
        Assert.Contains("100", str);
        Assert.Contains("2048", str);
        Assert.Contains("4096", str);
    }

    [Fact]
    public void TryParse_ReturnsFalse_WhenInputTooShort()
    {
        // arrange
        var bytes = new byte[10]; // not enough for EOCD64

        // act
        var success = Zip64EndOfCentralDirectory.TryParse(bytes, out var result);

        // assert
        Assert.False(success);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(false, true, true, true, true, true, true, true, true, true, "Signature")]
    [InlineData(true, false, true, true, true, true, true, true, true, true, "SizeOfEOCD")]
    [InlineData(true, true, false, true, true, true, true, true, true, true, "VersionMadeBy")]
    [InlineData(true, true, true, false, true, true, true, true, true, true, "VersionNeededToExtract")]
    [InlineData(true, true, true, true, false, true, true, true, true, true, "DiskNumber")]
    [InlineData(true, true, true, true, true, false, true, true, true, true, "CentralDirectoryStartDisk")]
    [InlineData(true, true, true, true, true, true, false, true, true, true, "TotalEntriesOnThisDisk")]
    [InlineData(true, true, true, true, true, true, true, false, true, true, "TotalEntries")]
    [InlineData(true, true, true, true, true, true, true, true, false, true, "CentralDirectorySize")]
    [InlineData(true, true, true, true, true, true, true, true, true, false, "CentralDirectoryOffset")]
    public void WriteTo_ThrowsIfRequiredFieldIsMissing(
        bool hasSig,
        bool hasSizeOfEOCD,
        bool hasVersionMadeBy,
        bool hasVersionNeeded,
        bool hasDiskNumber,
        bool hasStartDisk,
        bool hasTotalOnDisk,
        bool hasTotalEntries,
        bool hasDirSize,
        bool hasDirOffset,
        string expectedParam)
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory
        {
            Signature = hasSig ? 0x06064b50 : null,
            SizeOfEOCD = hasSizeOfEOCD ? 44UL : null,
            VersionMadeBy = hasVersionMadeBy ? (ushort)45 : null,
            VersionNeededToExtract = hasVersionNeeded ? (ushort)45 : null,
            DiskNumber = hasDiskNumber ? 0U : null,
            CentralDirectoryStartDisk = hasStartDisk ? 0U : null,
            TotalEntriesOnThisDisk = hasTotalOnDisk ? 1UL : null,
            TotalEntries = hasTotalEntries ? 1UL : null,
            CentralDirectorySize = hasDirSize ? 123UL : null,
            CentralDirectoryOffset = hasDirOffset ? 456UL : null,
        };

        // act
        var exception = Record.Exception(() => eocd.GetBytes(validate: true));

        // assert
        var ex = Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains(expectedParam, ex.Message);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory();

        // act
        var exception = Record.Exception(() => eocd.WriteTo(null!));

        // assert
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamNotWritable()
    {
        // arrange
        var eocd = new Zip64EndOfCentralDirectory();
        using var readOnly = new MemoryStream(new byte[64], writable: false);

        // act
        var exception = Record.Exception(() => eocd.WriteTo(readOnly));

        // assert
        Assert.IsType<ArgumentException>(exception);
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip64EndOfCentralDirectory result)
        => Zip64EndOfCentralDirectory.TryParse(bytes, out result);
}
