using System;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

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
        // arrange

        // act
        var actual = Zip64EndOfCentralDirectoryLocatorFactory.CreateLocator(
            zip64EndOfCentralDirectoryOffset: value);

        // assert
        Assert.Equal(value, (long?)actual.Zip64EndOfCentralDirectoryOffset);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        // arrange

        // act
        var actual = Zip64EndOfCentralDirectoryLocatorFactory.CreateLocator(
            zip64EndOfCentralDirectoryOffset: 0);

        // assert
        Assert.Equal(Zip64EndOfCentralDirectoryLocator.KnownSignature, actual.Signature);
        Assert.Equal((ushort)0, actual.CentralDirectoryStartDisk);
        Assert.Equal((ushort)1, actual.TotalDisks);
    }
}
