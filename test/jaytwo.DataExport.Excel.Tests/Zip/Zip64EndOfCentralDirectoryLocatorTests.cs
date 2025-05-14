using System;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip64EndOfCentralDirectoryLocatorTests
{
    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectSignature(uint value)
    {
        // arrange
        var locator = new Zip64EndOfCentralDirectoryLocator { Signature = value };

        // act
        var bytes = locator.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Signature);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(0x12345678UL)]
    [InlineData(0xFFFFFFFFUL)]
    [InlineData(0x0123456789ABCDEFUL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void WriteTo_WritesCorrectOffset(ulong value)
    {
        // arrange
        var locator = new Zip64EndOfCentralDirectoryLocator { Zip64EndOfCentralDirectoryOffset = value };

        // act
        var bytes = locator.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.Zip64EndOfCentralDirectoryOffset);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectCentralDirectoryStartDisk(uint value)
    {
        // arrange
        var locator = new Zip64EndOfCentralDirectoryLocator { CentralDirectoryStartDisk = value };

        // act
        var bytes = locator.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.CentralDirectoryStartDisk);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(0x12345678U)]
    [InlineData(0xFFFFFFFFU)]
    public void WriteTo_WritesCorrectCentralTotalDisks(uint value)
    {
        // arrange
        var locator = new Zip64EndOfCentralDirectoryLocator { TotalDisks = value };

        // act
        var bytes = locator.GetBytes(validate: false);

        // assert
        TryParse(bytes, out var parsed);
        Assert.Equal(value, parsed.TotalDisks);
    }

    [Fact]
    public void WriteTo_ThrowsIfStreamIsNull()
    {
        // arrange
        var locator = Zip64EndOfCentralDirectoryLocatorFactory.CreateLocator(0);

        // act & assert
        Assert.Throws<ArgumentNullException>(() => locator.WriteTo(null!));
    }

    [Fact]
    public void WriteTo_WritesExactly20Bytes()
    {
        // arrange
        var locator = new Zip64EndOfCentralDirectoryLocator();

        // act
        var bytes = locator.GetBytes(validate: false);

        // assert
        Assert.Equal(20, bytes.Length); // 4 (sig) + 4 (disk) + 8 (offset) + 4 (disks)
    }

    [Fact]
    public void ToString_IncludesOffsetValue()
    {
        // arrange
        ulong offset = 0xDEADBEEFCAFEBABE;
        var locator = Zip64EndOfCentralDirectoryLocatorFactory.CreateLocator(offset);

        // act
        var result = locator.ToString();

        // assert
        Assert.Contains(offset.ToString(), result);
    }

    [Theory]
    [InlineData(false, true, true, true, "Signature")]
    [InlineData(true, false, true, true, "CentralDirectoryStartDisk")]
    [InlineData(true, true, false, true, "Zip64EndOfCentralDirectoryOffset")]
    [InlineData(true, true, true, false, "TotalDisks")]
    public void WriteTo_ThrowsIfRequiredFieldIsMissing(
        bool hasSig,
        bool hasStartDisk,
        bool hasOffset,
        bool hasTotalDisks,
        string expectedParam)
    {
        // arrange
        var locator = new Zip64EndOfCentralDirectoryLocator
        {
            Signature = hasSig ? Zip64EndOfCentralDirectoryLocator.KnownSignature : null,
            CentralDirectoryStartDisk = hasStartDisk ? 0u : null,
            Zip64EndOfCentralDirectoryOffset = hasOffset ? 98765UL : null,
            TotalDisks = hasTotalDisks ? 1u : null,
        };

        // act
        var ex = Assert.Throws<InvalidOperationException>(() => locator.GetBytes(validate: true));

        // assert
        Assert.Contains(expectedParam, ex.Message);
    }

    // --- Helpers ---

    private static bool TryParse(byte[] bytes, out Zip64EndOfCentralDirectoryLocator result)
        => Zip64EndOfCentralDirectoryLocator.TryParse(bytes, out result);
}
