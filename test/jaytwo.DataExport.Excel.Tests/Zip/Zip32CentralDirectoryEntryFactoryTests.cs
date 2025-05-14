using System;
using jaytwo.DataExport.Excel.Zip;
using jaytwo.DataExport.Excel.Zip.Zip32;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip32CentralDirectoryEntryFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void CreateCentralDirectoryEntry_SetsCompressionMethodCorrectly(ushort value)
    {
        // arrange
        var factory = new Zip32CentralDirectoryEntryFactory();

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
        var typed = Assert.IsType<Zip32CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.CompressionMethod);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void CreateCentralDirectoryEntry_SetsCrc32Correctly(uint value)
    {
        // arrange
        var factory = new Zip32CentralDirectoryEntryFactory();

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
        var typed = Assert.IsType<Zip32CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void CreateCentralDirectoryEntry_SetsCompressedSizeCorrectly(uint value)
    {
        // arrange
        var factory = new Zip32CentralDirectoryEntryFactory();

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
        var typed = Assert.IsType<Zip32CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void CreateCentralDirectoryEntry_SetsUncompressedSizeCorrectly(uint value)
    {
        // arrange
        var factory = new Zip32CentralDirectoryEntryFactory();

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
        var typed = Assert.IsType<Zip32CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.UncompressedSize);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void CreateCentralDirectoryEntry_SetsFileNameAndLengthCorrectly(string value)
    {
        // arrange
        var factory = new Zip32CentralDirectoryEntryFactory();

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
        var typed = Assert.IsType<Zip32CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.FileName);
        Assert.Equal(value.Length, (int?)typed.FileNameLength);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void CreateCentralDirectoryEntry_SetsFileCommentAndLengthCorrectly(string value)
    {
        // arrange
        var factory = new Zip32CentralDirectoryEntryFactory();

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
        var typed = Assert.IsType<Zip32CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.FileComment);
        Assert.Equal(value.Length, (int?)typed.FileCommentLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void CreateCentralDirectoryEntry_SetsLocalHeaderOffsetCorrectly(uint value)
    {
        // arrange
        var factory = new Zip32CentralDirectoryEntryFactory();

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
        var typed = Assert.IsType<Zip32CentralDirectoryEntry>(actual);
        Assert.Equal(value, typed.LocalHeaderOffset);
    }

    [Fact]
    public void ExtraField_Matches()
    {
        // arrange
        var expected = new byte[] { 1, 2, 3, 4 };

        // act
        var actual = Zip32CentralDirectoryEntryFactory.CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0,
            extraField: expected);

        // assert
        Assert.Equal(expected, actual.ExtraField);
        Assert.Equal((ushort)expected.Length, actual.ExtraFieldLength);
    }

    [Fact]
    public void CreateCentralDirectoryEntry_SetsDefaultZipFieldsCorrectly()
    {
        // arrange
        var factory = new Zip32CentralDirectoryEntryFactory();

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
        var typed = Assert.IsType<Zip32CentralDirectoryEntry>(actual);
        Assert.Equal(Zip32CentralDirectoryEntry.KnownSignature, typed.Signature);
        Assert.Equal(ZipConstants.Versions.Version20, typed.VersionMadeBy);
        Assert.Equal(ZipConstants.Versions.Version20, typed.VersionNeededToExtract);
        Assert.Equal(ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows, typed.GeneralPurposeBitFlag);
        Assert.Equal((ushort)0, typed.LastModDate);
        Assert.Equal((ushort)0, typed.LastModTime);
        Assert.Equal((ushort)0, typed.DiskNumberStart);
        Assert.Equal((ushort)0, typed.InternalFileAttributes);
        Assert.Equal(0u, typed.ExternalFileAttributes);
        Assert.Empty(typed.ExtraField);
        Assert.Equal((ushort)0, typed.ExtraFieldLength);
    }
}
