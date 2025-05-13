using System;
using jaytwo.StreamingExcelExport.Zip;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

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
        var part = Zip64EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: value,
            centralDirectorySize: 0,
            centralDirectoryOffset: 0);

        var entry = Assert.IsType<Zip64EndOfCentralDirectory>(part);
        Assert.Equal(value, (int?)entry.TotalEntries);
        Assert.Equal(value, (int?)entry.TotalEntriesOnThisDisk);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void CentralDirectoryOffset_Matches(long value)
    {
        var part = Zip64EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: 0,
            centralDirectoryOffset: value);

        var entry = Assert.IsType<Zip64EndOfCentralDirectory>(part);
        Assert.Equal(value, (long?)entry.CentralDirectoryOffset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void CentralDirectorySize_Matches(long value)
    {
        var part = Zip64EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: value,
            centralDirectoryOffset: 0);

        var entry = Assert.IsType<Zip64EndOfCentralDirectory>(part);
        Assert.Equal(value, (long?)entry.CentralDirectorySize);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        var part = Zip64EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: 0,
            centralDirectoryOffset: 0);

        var entry = Assert.IsType<Zip64EndOfCentralDirectory>(part);
        Assert.Equal(Zip64EndOfCentralDirectory.KnownSignature, entry.Signature);
        Assert.Equal(Zip64EndOfCentralDirectory.FixedSizeOfEOCD, entry.SizeOfEOCD);
        Assert.Equal(ZipConstants.Versions.Version45, entry.VersionMadeBy);
        Assert.Equal(ZipConstants.Versions.Version45, entry.VersionNeededToExtract);
        Assert.Equal((ushort)0, entry.DiskNumber);
        Assert.Equal((ushort)0, entry.CentralDirectoryStartDisk);
    }
}
