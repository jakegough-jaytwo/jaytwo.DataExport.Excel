using System;
using jaytwo.StreamingExcelExport.Zip;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32LocalFileHeaderFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void CompressionMethod_Matches(ushort value)
    {
        var factory = new Zip32LocalFileHeaderFactory();
        var part = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: value,
            fileName: string.Empty);

        var entry = Assert.IsType<Zip32LocalFileHeader>(part);
        Assert.Equal(value, entry.CompressionMethod);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void FileName_Matches(string value)
    {
        var factory = new Zip32LocalFileHeaderFactory();
        var part = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: 0,
            fileName: value);

        var entry = Assert.IsType<Zip32LocalFileHeader>(part);
        Assert.Equal(value, entry.FileName);
        Assert.Equal(value.Length, (int?)entry.FileNameLength);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        var factory = new Zip32LocalFileHeaderFactory();
        var part = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: 0,
            fileName: string.Empty);

        var entry = Assert.IsType<Zip32LocalFileHeader>(part);
        Assert.Equal(Zip32LocalFileHeader.KnownSignature, entry.Signature);
        Assert.Equal(ZipConstants.Versions.Version20, entry.VersionNeededToExtract);
        Assert.Equal(ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows, entry.GeneralPurposeBitFlag);
        Assert.Equal((ushort)0, entry.LastModDate);
        Assert.Equal((ushort)0, entry.LastModTime);
        Assert.Equal((ushort)0, entry.Crc32);
        Assert.Equal((ushort)0, entry.CompressedSize);
        Assert.Equal((ushort)0, entry.UncompressedSize);
        Assert.Empty(entry.ExtraField);
        Assert.Equal((ushort)0, entry.ExtraFieldLength);
    }
}
