using System;
using jaytwo.DataExport.Excel.Zip;
using jaytwo.DataExport.Excel.Zip.Zip32;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip32LocalFileHeaderFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void CompressionMethod_Matches(ushort value)
    {
        // arrange
        var factory = new Zip32LocalFileHeaderFactory();

        // act
        var actual = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: value,
            fileName: string.Empty);

        // assert
        var typed = Assert.IsType<Zip32LocalFileHeader>(actual);
        Assert.Equal(value, typed.CompressionMethod);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void FileName_Matches(string value)
    {
        // arrange
        var factory = new Zip32LocalFileHeaderFactory();

        // act
        var actual = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: 0,
            fileName: value);

        // assert
        var typed = Assert.IsType<Zip32LocalFileHeader>(actual);
        Assert.Equal(value, typed.FileName);
        Assert.Equal(value.Length, (int?)typed.FileNameLength);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        // arrange
        var factory = new Zip32LocalFileHeaderFactory();

        // act
        var actual = ((ILocalFileHeaderFactory)factory).CreateLocalFileHeader(
            compressionMethod: 0,
            fileName: string.Empty);

        // assert
        var typed = Assert.IsType<Zip32LocalFileHeader>(actual);
        Assert.Equal(Zip32LocalFileHeader.KnownSignature, typed.Signature);
        Assert.Equal(ZipConstants.Versions.Version20, typed.VersionNeededToExtract);
        Assert.Equal(ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows, typed.GeneralPurposeBitFlag);
        Assert.Equal((ushort)0, typed.LastModDate);
        Assert.Equal((ushort)0, typed.LastModTime);
        Assert.Equal((ushort)0, typed.Crc32);
        Assert.Equal((ushort)0, typed.CompressedSize);
        Assert.Equal((ushort)0, typed.UncompressedSize);
        Assert.NotNull(typed.ExtraField);
        Assert.Empty(typed.ExtraField!);
        Assert.Equal((ushort)0, typed.ExtraFieldLength);
    }
}
