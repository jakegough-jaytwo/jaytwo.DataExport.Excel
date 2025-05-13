using System;
using jaytwo.StreamingExcelExport.Zip;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64DataDescriptorFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void Crc32_Matches(uint value)
    {
        var factory = new Zip64DataDescriptorFactory();
        var part = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: value,
            compressedSize: 0,
            uncompressedSize: 0);

        var entry = Assert.IsType<Zip64DataDescriptor>(part);
        Assert.Equal(value, entry.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void CompressedSize_Matches(long value)
    {
        var factory = new Zip64DataDescriptorFactory();
        var part = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: value,
            uncompressedSize: 0);

        var entry = Assert.IsType<Zip64DataDescriptor>(part);
        Assert.Equal(value, (long?)entry.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void UncompressedSize_Matches(long value)
    {
        var factory = new Zip64DataDescriptorFactory();
        var part = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: value);

        var entry = Assert.IsType<Zip64DataDescriptor>(part);
        Assert.Equal(value, (long?)entry.UncompressedSize);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        var factory = new Zip64DataDescriptorFactory();
        var part = ((IDataDescriptorFactory)factory).CreateDataDescriptor(
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0);

        var entry = Assert.IsType<Zip64DataDescriptor>(part);
        Assert.Equal(Zip64DataDescriptor.KnownSignature, entry.Signature);
    }
}
