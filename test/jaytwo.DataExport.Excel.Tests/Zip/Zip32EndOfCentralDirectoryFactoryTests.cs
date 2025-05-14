using System;
using jaytwo.DataExport.Excel.Zip;
using jaytwo.DataExport.Excel.Zip.Zip32;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class Zip32EndOfCentralDirectoryFactoryTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    public void TotalEntries_Matches(ushort value)
    {
        // arrange

        // act
        var actual = Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: value,
            centralDirectorySize: 0,
            centralDirectoryOffset: 0,
            comment: string.Empty);

        // assert
        var typed = Assert.IsType<Zip32EndOfCentralDirectory>(actual);
        Assert.Equal(value, (int?)typed.TotalEntries);
        Assert.Equal(value, (int?)typed.TotalEntriesOnThisDisk);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x1234)]
    [InlineData(0xFFFF)]
    [InlineData(0x12345678)]
    public void CentralDirectoryOffset_Matches(uint value)
    {
        // arrange

        // act
        var actual = Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: 0,
            centralDirectoryOffset: value,
            comment: string.Empty);

        // assert
        var typed = Assert.IsType<Zip32EndOfCentralDirectory>(actual);
        Assert.Equal(value, typed.CentralDirectoryOffset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x12345678)]
    [InlineData(0xFFFFFFFF)]
    public void CentralDirectorySize_Matches(uint value)
    {
        // arrange

        // act
        var actual = Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: value,
            centralDirectoryOffset: 0,
            comment: string.Empty);

        // assert
        var typed = Assert.IsType<Zip32EndOfCentralDirectory>(actual);
        Assert.Equal(value, typed.CentralDirectorySize);
    }

    [Theory]
    [InlineData("")]
    [InlineData("banana")]
    public void FileComment_Matches(string value)
    {
        // arrange

        // act
        var actual = Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: 0,
            centralDirectoryOffset: 0,
            comment: value);

        // assert
        var typed = Assert.IsType<Zip32EndOfCentralDirectory>(actual);
        Assert.Equal(value, typed.Comment);
        Assert.Equal(value.Length, (int?)typed.CommentLength);
    }

    [Fact]
    public void DefaultPropertiesArePopulatedWithExpectedValues()
    {
        // arrange

        // act
        var actual = Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
            totalEntries: 0,
            centralDirectorySize: 0,
            centralDirectoryOffset: 0,
            comment: string.Empty);

        // assert
        var typed = Assert.IsType<Zip32EndOfCentralDirectory>(actual);
        Assert.Equal(Zip32EndOfCentralDirectory.KnownSignature, typed.Signature);
        Assert.Equal((ushort)0, typed.DiskNumber);
        Assert.Equal((ushort)0, typed.CentralDirectoryStartDisk);
    }
}
