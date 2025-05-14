using System;
using jaytwo.DataExport.Excel.Zip;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip64DataDescriptorFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void Crc32_Matches(uint value)
    {
        // arrange
        var factory = new Zip64DataDescriptorFactory();

        // act
        var actual = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: value,
            compressedSize: 0,
            uncompressedSize: 0);

        // assert
        var typed = Assert.IsType<Zip64DataDescriptor>(actual);
        Assert.Equal(value, typed.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void CompressedSize_Matches(long value)
    {
        // arrange
        var factory = new Zip64DataDescriptorFactory();

        // act
        var actual = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: value,
            uncompressedSize: 0);

        // assert
        var typed = Assert.IsType<Zip64DataDescriptor>(actual);
        Assert.Equal(value, (long?)typed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void UncompressedSize_Matches(long value)
    {
        // arrange
        var factory = new Zip64DataDescriptorFactory();

        // act
        var actual = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: value);

        // assert
        var typed = Assert.IsType<Zip64DataDescriptor>(actual);
        Assert.Equal(value, (long?)typed.UncompressedSize);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        // arrange
        var factory = new Zip64DataDescriptorFactory();

        // act
        var actual = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0);

        // assert
        var typed = Assert.IsType<Zip64DataDescriptor>(actual);
        Assert.Equal(Zip64DataDescriptor.KnownSignature, typed.Signature);
    }
}
