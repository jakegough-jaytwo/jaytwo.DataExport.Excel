using System;
using jaytwo.DataExport.Excel.Zip;
using jaytwo.DataExport.Excel.Zip.Zip32;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip32DataDescriptorFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void Crc32_Matches(uint value)
    {
        // arrange
        var factory = new Zip32DataDescriptorFactory();

        // act
        var actual = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: value,
            compressedSize: 0,
            uncompressedSize: 0);

        // assert
        var typed = Assert.IsType<Zip32DataDescriptor>(actual);
        Assert.Equal(value, typed.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void CompressedSize_Matches(uint value)
    {
        // arrange
        var factory = new Zip32DataDescriptorFactory();

        // act
        var actual = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: value,
            uncompressedSize: 0);

        // assert
        var typed = Assert.IsType<Zip32DataDescriptor>(actual);
        Assert.Equal(value, typed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void UncompressedSize_Matches(uint value)
    {
        // arrange
        var factory = new Zip32DataDescriptorFactory();

        // act
        var actual = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: value);

        // assert
        var typed = Assert.IsType<Zip32DataDescriptor>(actual);
        Assert.Equal(value, typed.UncompressedSize);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        // arrange
        var factory = new Zip32DataDescriptorFactory();

        // act
        var actual = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0);

        // assert
        var typed = Assert.IsType<Zip32DataDescriptor>(actual);
        Assert.Equal(Zip32DataDescriptor.KnownSignature, typed.Signature);
    }
}
