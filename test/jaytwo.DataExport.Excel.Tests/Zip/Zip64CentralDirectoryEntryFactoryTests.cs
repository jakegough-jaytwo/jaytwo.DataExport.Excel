using System;
using jaytwo.DataExport.Excel.Zip;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip64CentralDirectoryEntryFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void CompressionMethod_Matches(ushort value)
    {
        // arrange
        var factory = new Zip64CentralDirectoryEntryFactory();

        // act
        var actual = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: value,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        // assert
        var typed = Assert.IsType<Zip64CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.CompressionMethod);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void Crc32_Matches(uint value)
    {
        // arrange
        var factory = new Zip64CentralDirectoryEntryFactory();

        // act
        var actual = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: value,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        // assert
        var typed = Assert.IsType<Zip64CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void Zip64CompressedSize_Matches(long value)
    {
        // arrange
        var factory = new Zip64CentralDirectoryEntryFactory();

        // act
        var actual = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: value,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        // assert
        var typed = Assert.IsType<Zip64CentralDirectoryEntry>(actual);
        Assert.Equal(value, (long?)typed.Zip64CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void Zip64UncompressedSize_Matches(long value)
    {
        // arrange
        var factory = new Zip64CentralDirectoryEntryFactory();

        // act
        var actual = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: value,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        // assert
        var typed = Assert.IsType<Zip64CentralDirectoryEntry>(actual);
        Assert.Equal(value, (long?)typed.Zip64UncompressedSize);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void FileName_Matches(string value)
    {
        // arrange
        var factory = new Zip64CentralDirectoryEntryFactory();

        // act
        var actual = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: value,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        // assert
        var typed = Assert.IsType<Zip64CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.FileName);
        Assert.Equal(value.Length, (int?)typed.FileNameLength);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void FileComment_Matches(string value)
    {
        // arrange
        var factory = new Zip64CentralDirectoryEntryFactory();

        // act
        var actual = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: value,
            localHeaderOffset: 0);

        // assert
        var typed = Assert.IsType<Zip64CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.FileComment);
        Assert.Equal(value.Length, (int?)typed.FileCommentLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    [InlineData(0x123456789ABCD)]
    [InlineData(0x0FFFFFFFFFFFFFFF)]
    public void LocalHeaderOffset_Matches(long value)
    {
        // arrange
        var factory = new Zip64CentralDirectoryEntryFactory();

        // act
        var actual = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: value);

        // assert
        var typed = Assert.IsType<Zip64CentralDirectoryEntry>(actual);
        Assert.Equal(value, (long?)typed.Zip64LocalHeaderOffset);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        // arrange
        var factory = new Zip64CentralDirectoryEntryFactory();

        // act
        var actual = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        // assert
        var typed = Assert.IsType<Zip64CentralDirectoryEntry>(actual);
        Assert.Equal(Zip64CentralDirectoryEntry.KnownSignature, typed.Signature);
        Assert.Equal(ZipConstants.Versions.Version45, typed.VersionMadeBy);
        Assert.Equal(ZipConstants.Versions.Version45, typed.VersionNeededToExtract);
        Assert.Equal(ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows, typed.GeneralPurposeBitFlag);
        Assert.Equal((ushort)0, typed.LastModDate);
        Assert.Equal((ushort)0, typed.LastModTime);
        Assert.Equal((ushort)0, typed.DiskNumberStart);
        Assert.Equal((ushort)0, typed.InternalFileAttributes);
        Assert.Equal(0u, typed.ExternalFileAttributes);
        Assert.Equal(Zip64CentralDirectoryEntryFactory.SeeZip64ExtraFields, typed.CompressedSize);
        Assert.Equal(Zip64CentralDirectoryEntryFactory.SeeZip64ExtraFields, typed.UncompressedSize);
        Assert.Equal(Zip64CentralDirectoryEntryFactory.SeeZip64ExtraFields, typed.LocalHeaderOffset);
        Assert.True(typed.HasValidZip64ExtraField);
        Assert.Equal(Zip64CentralDirectoryEntry.Zip64ExtraFieldHeaderId, typed.ParsedZip64HeaderId);
    }
}
