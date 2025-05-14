using System;
using jaytwo.DataExport.Excel.Zip;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip64EndOfCentralDirectoryFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    [InlineData(0x12345678)]
    [InlineData(0x0FFFFFFF)]
    public void TotalEntries_Matches(int value)
    {
        // arrange

        // act
        var actual = Zip64EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: value,
            centralDirectorySize: 0,
            centralDirectoryOffset: 0);

        // assert
        var typed = Assert.IsType<Zip64EndOfCentralDirectory>(actual);
        Assert.Equal(value, (int?)typed.TotalEntries);
        Assert.Equal(value, (int?)typed.TotalEntriesOnThisDisk);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void CentralDirectoryOffset_Matches(long value)
    {
        // arrange

        // act
        var actual = Zip64EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: 0,
            centralDirectoryOffset: value);

        // assert
        var typed = Assert.IsType<Zip64EndOfCentralDirectory>(actual);
        Assert.Equal(value, (long?)typed.CentralDirectoryOffset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void CentralDirectorySize_Matches(long value)
    {
        // arrange

        // act
        var actual = Zip64EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: value,
            centralDirectoryOffset: 0);

        // assert
        var typed = Assert.IsType<Zip64EndOfCentralDirectory>(actual);
        Assert.Equal(value, (long?)typed.CentralDirectorySize);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        // arrange

        // act
        var actual = Zip64EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: 0,
            centralDirectoryOffset: 0);

        // assert
        var typed = Assert.IsType<Zip64EndOfCentralDirectory>(actual);
        Assert.Equal(Zip64EndOfCentralDirectory.KnownSignature, typed.Signature);
        Assert.Equal(Zip64EndOfCentralDirectory.FixedSizeOfEOCD, typed.SizeOfEOCD);
        Assert.Equal(ZipConstants.Versions.Version45, typed.VersionMadeBy);
        Assert.Equal(ZipConstants.Versions.Version45, typed.VersionNeededToExtract);
        Assert.Equal((ushort)0, typed.DiskNumber);
        Assert.Equal((ushort)0, typed.CentralDirectoryStartDisk);
    }
}
