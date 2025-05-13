using System;
using jaytwo.StreamingExcelExport.Zip;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32DataDescriptorFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void Crc32_Matches(uint value)
    {
        var factory = new Zip32DataDescriptorFactory();
        var part = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: value,
            compressedSize: 0,
            uncompressedSize: 0);

        var entry = Assert.IsType<Zip32DataDescriptor>(part);
        Assert.Equal(value, entry.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void CompressedSize_Matches(uint value)
    {
        var factory = new Zip32DataDescriptorFactory();
        var part = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: value,
            uncompressedSize: 0);

        var entry = Assert.IsType<Zip32DataDescriptor>(part);
        Assert.Equal(value, entry.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void UncompressedSize_Matches(uint value)
    {
        var factory = new Zip32DataDescriptorFactory();
        var part = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: value);

        var entry = Assert.IsType<Zip32DataDescriptor>(part);
        Assert.Equal(value, entry.UncompressedSize);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        var factory = new Zip32DataDescriptorFactory();
        var part = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0);

        var entry = Assert.IsType<Zip32DataDescriptor>(part);
        Assert.Equal(Zip32DataDescriptor.KnownSignature, entry.Signature);
    }
}
