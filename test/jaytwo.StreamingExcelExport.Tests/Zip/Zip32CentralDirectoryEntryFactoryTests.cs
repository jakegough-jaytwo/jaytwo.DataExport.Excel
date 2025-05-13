using System;
using jaytwo.StreamingExcelExport.Zip;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32CentralDirectoryEntryFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void CompressionMethod_Matches(ushort value)
    {
        var factory = new Zip32CentralDirectoryEntryFactory();
        var part = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: value,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        var entry = Assert.IsType<Zip32CentralDirectoryEntry>(part);
        Assert.Equal(value, entry.CompressionMethod);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void Crc32_Matches(uint value)
    {
        var factory = new Zip32CentralDirectoryEntryFactory();
        var part = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: value,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        var entry = Assert.IsType<Zip32CentralDirectoryEntry>(part);
        Assert.Equal(value, entry.Crc32);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void CompressedSize_Matches(uint value)
    {
        var factory = new Zip32CentralDirectoryEntryFactory();
        var part = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: value,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        var entry = Assert.IsType<Zip32CentralDirectoryEntry>(part);
        Assert.Equal(value, entry.CompressedSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void UncompressedSize_Matches(uint value)
    {
        var factory = new Zip32CentralDirectoryEntryFactory();
        var part = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: value,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        var entry = Assert.IsType<Zip32CentralDirectoryEntry>(part);
        Assert.Equal(value, entry.UncompressedSize);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void FileName_Matches(string value)
    {
        var factory = new Zip32CentralDirectoryEntryFactory();
        var part = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: value,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        var entry = Assert.IsType<Zip32CentralDirectoryEntry>(part);
        Assert.Equal(value, entry.FileName);
        Assert.Equal(value.Length, (int?)entry.FileNameLength);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void FileComment_Matches(string value)
    {
        var factory = new Zip32CentralDirectoryEntryFactory();
        var part = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: value,
            localHeaderOffset: 0);

        var entry = Assert.IsType<Zip32CentralDirectoryEntry>(part);
        Assert.Equal(value, entry.FileComment);
        Assert.Equal(value.Length, (int?)entry.FileCommentLength);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void LocalHeaderOffset_Matches(uint value)
    {
        var factory = new Zip32CentralDirectoryEntryFactory();
        var part = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: value);

        var entry = Assert.IsType<Zip32CentralDirectoryEntry>(part);
        Assert.Equal(value, entry.LocalHeaderOffset);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        var factory = new Zip32CentralDirectoryEntryFactory();
        var part = ((ICentralDirectoryEntryFactory)factory).CreateCentralDirectoryEntry(
            compressionMethod: 0,
            crc32: 0,
            compressedSize: 0,
            uncompressedSize: 0,
            fileName: string.Empty,
            fileComment: string.Empty,
            localHeaderOffset: 0);

        var entry = Assert.IsType<Zip32CentralDirectoryEntry>(part);
        Assert.Equal(Zip32CentralDirectoryEntry.KnownSignature, entry.Signature);
        Assert.Equal(ZipConstants.Versions.Version20, entry.VersionMadeBy);
        Assert.Equal(ZipConstants.Versions.Version20, entry.VersionNeededToExtract);
        Assert.Equal(ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows, entry.GeneralPurposeBitFlag);
        Assert.Equal((ushort)0, entry.LastModDate);
        Assert.Equal((ushort)0, entry.LastModTime);
        Assert.Equal((ushort)0, entry.DiskNumberStart);
        Assert.Equal((ushort)0, entry.InternalFileAttributes);
        Assert.Equal(0u, entry.ExternalFileAttributes);
        Assert.Empty(entry.ExtraField);
        Assert.Equal((ushort)0, entry.ExtraFieldLength);
    }
}
