using System;
using jaytwo.StreamingExcelExport.Zip;
using jaytwo.StreamingExcelExport.Zip.Zip64;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip64LocalFileHeaderFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void CompressionMethod_Matches(ushort value)
    {
        var factory = new Zip64LocalFileHeaderFactory();
        var part = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: value,
            fileName: string.Empty);

        var entry = Assert.IsType<Zip64LocalFileHeader>(part);
        Assert.Equal(value, entry.CompressionMethod);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void FileName_Matches(string value)
    {
        var factory = new Zip64LocalFileHeaderFactory();
        var part = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: 0,
            fileName: value);

        var entry = Assert.IsType<Zip64LocalFileHeader>(part);
        Assert.Equal(value, entry.FileName);
        Assert.Equal(value.Length, (int?)entry.FileNameLength);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        var factory = new Zip64LocalFileHeaderFactory();
        var part = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: 0,
            fileName: string.Empty);

        var entry = Assert.IsType<Zip64LocalFileHeader>(part);
        Assert.Equal(Zip64LocalFileHeader.KnownSignature, entry.Signature);
        Assert.Equal(ZipConstants.Versions.Version45, entry.VersionNeededToExtract);
        Assert.Equal(ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows, entry.GeneralPurposeBitFlag);
        Assert.Equal((ushort)0, entry.LastModDate);
        Assert.Equal((ushort)0, entry.LastModTime);
        Assert.Equal((ushort)0, entry.Crc32);
        Assert.Equal(Zip64LocalFileHeaderFactory.SeeZip64ExtraFields, entry.CompressedSize);
        Assert.Equal(Zip64LocalFileHeaderFactory.SeeZip64ExtraFields, entry.UncompressedSize);

        Assert.True(entry.HasValidZip64ExtraField);
        Assert.Equal((ushort)0, entry.Zip64CompressedSize);
        Assert.Equal((ushort)0, entry.Zip64UncompressedSize);
        Assert.Equal(Zip64LocalFileHeader.Zip64ExtraFieldHeaderId, entry.ParsedZip64HeaderId);
    }
}
