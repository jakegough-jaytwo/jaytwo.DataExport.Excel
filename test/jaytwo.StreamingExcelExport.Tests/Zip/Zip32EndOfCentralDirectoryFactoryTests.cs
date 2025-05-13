using System;
using jaytwo.StreamingExcelExport.Zip;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class Zip32EndOfCentralDirectoryFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void TotalEntries_Matches(ushort value)
    {
        var part = Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: value,
            centralDirectorySize: 0,
            centralDirectoryOffset: 0,
            comment: string.Empty);

        var entry = Assert.IsType<Zip32EndOfCentralDirectory>(part);
        Assert.Equal(value, (int?)entry.TotalEntries);
        Assert.Equal(value, (int?)entry.TotalEntriesOnThisDisk);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    [InlineData(0x12345678)]
    public void CentralDirectoryOffset_Matches(uint value)
    {
        var part = Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: 0,
            centralDirectoryOffset: value,
            comment: string.Empty);

        var entry = Assert.IsType<Zip32EndOfCentralDirectory>(part);
        Assert.Equal(value, entry.CentralDirectoryOffset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void CentralDirectorySize_Matches(uint value)
    {
        var part = Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: value,
            centralDirectoryOffset: 0,
            comment: string.Empty);

        var entry = Assert.IsType<Zip32EndOfCentralDirectory>(part);
        Assert.Equal(value, entry.CentralDirectorySize);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void FileComment_Matches(string value)
    {
        var part = Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: 0,
            centralDirectoryOffset: 0,
            comment: value);

        var entry = Assert.IsType<Zip32EndOfCentralDirectory>(part);
        Assert.Equal(value, entry.Comment);
        Assert.Equal(value.Length, (int?)entry.CommentLength);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        var part = Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: 0,
            centralDirectoryOffset: 0,
            comment: string.Empty);

        var entry = Assert.IsType<Zip32EndOfCentralDirectory>(part);
        Assert.Equal(Zip32EndOfCentralDirectory.KnownSignature, entry.Signature);
        Assert.Equal((ushort)0, entry.DiskNumber);
        Assert.Equal((ushort)0, entry.CentralDirectoryStartDisk);
    }
}
