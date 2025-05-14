using System;
using jaytwo.DataExport.Excel.Zip;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip64LocalFileHeaderFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void CompressionMethod_Matches(ushort value)
    {
        // arrange
        var factory = new Zip64LocalFileHeaderFactory();

        // act
        var actual = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: value,
            fileName: string.Empty);

        // assert
        var typed = Assert.IsType<Zip64LocalFileHeader>(actual);
        Assert.Equal(value, typed.CompressionMethod);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void FileName_Matches(string value)
    {
        // arrange
        var factory = new Zip64LocalFileHeaderFactory();

        // act
        var actual = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: 0,
            fileName: value);

        // assert
        var typed = Assert.IsType<Zip64LocalFileHeader>(actual);
        Assert.Equal(value, typed.FileName);
        Assert.Equal(value.Length, (int?)typed.FileNameLength);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        // arrange
        var factory = new Zip64LocalFileHeaderFactory();

        // act
        var actual = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: 0,
            fileName: string.Empty);

        // assert
        var typed = Assert.IsType<Zip64LocalFileHeader>(actual);
        Assert.Equal(Zip64LocalFileHeader.KnownSignature, typed.Signature);
        Assert.Equal(ZipConstants.Versions.Version45, typed.VersionNeededToExtract);
        Assert.Equal(ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows, typed.GeneralPurposeBitFlag);
        Assert.Equal((ushort)0, typed.LastModDate);
        Assert.Equal((ushort)0, typed.LastModTime);
        Assert.Equal((ushort)0, typed.Crc32);
        Assert.Equal(Zip64LocalFileHeaderFactory.SeeZip64ExtraFields, typed.CompressedSize);
        Assert.Equal(Zip64LocalFileHeaderFactory.SeeZip64ExtraFields, typed.UncompressedSize);

        Assert.True(typed.HasValidZip64ExtraField);
        Assert.Equal((ushort)0, typed.Zip64CompressedSize);
        Assert.Equal((ushort)0, typed.Zip64UncompressedSize);
        Assert.Equal(Zip64LocalFileHeader.Zip64ExtraFieldHeaderId, typed.ParsedZip64HeaderId);
    }
}
