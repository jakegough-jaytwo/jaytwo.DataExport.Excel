using System;
using jaytwo.StreamingExcelExport.Zip;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64EndOfCentralDirectoryLocatorFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void Zip64EndOfCentralDirectoryOffset_Matches(long value)
    {
        var part = Zip64EndOfCentralDirectoryLocatorFactory.CreateLocator(
            zip64EndOfCentralDirectoryOffset: value);

        var entry = Assert.IsType<Zip64EndOfCentralDirectoryLocator>(part);
        Assert.Equal(value, (long?)entry.Zip64EndOfCentralDirectoryOffset);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        var part = Zip64EndOfCentralDirectoryLocatorFactory.CreateLocator(
            zip64EndOfCentralDirectoryOffset: 0);

        var entry = Assert.IsType<Zip64EndOfCentralDirectoryLocator>(part);
        Assert.Equal(Zip64EndOfCentralDirectoryLocator.KnownSignature, entry.Signature);
        Assert.Equal((ushort)0, entry.CentralDirectoryStartDisk);
        Assert.Equal((ushort)1, entry.TotalDisks);
    }
}
