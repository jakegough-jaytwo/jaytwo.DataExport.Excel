using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Force.Crc32;
using jaytwo.DataExport.Excel.Zip;
using jaytwo.DataExport.Excel.Zip.Zip32;
using jaytwo.DataExport.Excel.Zip.Zip64;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class ZipStructureValidationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CentralDirectoryCrcMatchesContent(bool useZip64)
    {
        // arrange
        byte[] content = GetRandomBytes();

        // act
        var zipBytes = await CreateZipPackageBytes(content, useZip64: useZip64);

        // assert
        // Find central directory entry
        var centralDirPos = FindSignature(zipBytes, Zip32CentralDirectoryEntry.KnownSignature);
        Assert.True(centralDirPos >= 0, "Central directory not found");
        Assert.Equal(Zip32CentralDirectoryEntry.KnownSignature, BitConverter.ToUInt32(zipBytes, centralDirPos));

        // Extract stored CRC32 from central directory entry
        uint storedCrc = BitConverter.ToUInt32(zipBytes, centralDirPos + Zip32CentralDirectoryEntry.Offsets.CrcOffset);

        // Compute actual CRC32 from original content
        uint actualCrc = Crc32Algorithm.Compute(content);

        Assert.Equal(actualCrc, storedCrc);
    }

    [Fact]
    public async Task Zip32_WritesCorrectFileAndCentralDirectoryHeaders()
    {
        // arrange
        string filename = "test.txt";
        byte[] content = GetRandomBytes();

        // act
        byte[] zipBytes = await CreateZipPackageBytes(content, filename, useZip64: false);

        // assert
        // Find all signature positions
        var localHeaderPos = FindSignature(zipBytes, Zip32LocalFileHeader.KnownSignature);
        var centralDirPos = FindSignature(zipBytes, Zip32CentralDirectoryEntry.KnownSignature);
        var eocdPos = FindSignature(zipBytes, Zip32EndOfCentralDirectory.KnownSignature);

        Assert.True(localHeaderPos >= 0, "Local file header not found");
        Assert.True(centralDirPos > localHeaderPos, "Central directory must follow local file header");
        Assert.True(eocdPos > centralDirPos, "EOCD must follow central directory");

        // Verify local file header fields (at least filename length and file name)
        ushort nameLength = BitConverter.ToUInt16(zipBytes, localHeaderPos + 26);
        string fileNameInHeader = Encoding.UTF8.GetString(zipBytes, localHeaderPos + 30, nameLength);
        Assert.Equal(filename, fileNameInHeader);

        // Verify central directory entry matches file name
        ushort centralNameLength = BitConverter.ToUInt16(zipBytes, centralDirPos + 28);
        string fileNameInCentral = Encoding.UTF8.GetString(zipBytes, centralDirPos + 46, centralNameLength);
        Assert.Equal(filename, fileNameInCentral);

        // Verify EOCD total entries is 1
        ushort totalEntries = BitConverter.ToUInt16(zipBytes, eocdPos + 10);
        Assert.Equal(1, totalEntries);
    }

    [Fact]
    public async Task Zip64_WritesCorrectStructure()
    {
        // arrange
        string filename = "zip64.txt";
        byte[] content = GetRandomBytes();

        // act
        byte[] zipBytes = await CreateZipPackageBytes(content, filename, useZip64: true);

        // assert
        // Signature positions
        int localHeaderPos = FindSignature(zipBytes, Zip64LocalFileHeader.KnownSignature);
        int centralDirPos = FindSignature(zipBytes, Zip64CentralDirectoryEntry.KnownSignature);
        int zip64EocdPos = FindSignature(zipBytes, Zip64EndOfCentralDirectory.KnownSignature);
        int zip64LocatorPos = FindSignature(zipBytes, Zip64EndOfCentralDirectoryLocator.KnownSignature);
        int eocdPos = FindSignature(zipBytes, Zip32EndOfCentralDirectory.KnownSignature);

        Assert.True(localHeaderPos >= 0, "Local file header not found");
        Assert.True(centralDirPos > localHeaderPos, "Central directory must follow local file header");
        Assert.True(zip64EocdPos > centralDirPos, "Zip64 EOCD must follow central directory");
        Assert.True(zip64LocatorPos > zip64EocdPos, "Zip64 locator must follow Zip64 EOCD");
        Assert.True(eocdPos > zip64LocatorPos, "Standard EOCD must follow Zip64 locator");

        // Verify file name in local file header
        ushort nameLength = BitConverter.ToUInt16(zipBytes, localHeaderPos + 26);
        string fileNameInHeader = Encoding.UTF8.GetString(zipBytes, localHeaderPos + 30, nameLength);
        Assert.Equal(filename, fileNameInHeader);

        // Verify file name in central directory
        ushort centralNameLength = BitConverter.ToUInt16(zipBytes, centralDirPos + 28);
        string fileNameInCentral = Encoding.UTF8.GetString(zipBytes, centralDirPos + 46, centralNameLength);
        Assert.Equal(filename, fileNameInCentral);

        // EOCD entry count should be 0xFFFF (overflow marker in ZIP32 EOCD)
        ushort totalEntries = BitConverter.ToUInt16(zipBytes, eocdPos + 10);
        Assert.Equal(0xFFFF, totalEntries);
    }

    [Theory]
    [InlineData(256)]
    [InlineData(257)]
    [InlineData(65536)]
    [InlineData(65537)]
    public async Task Zip32_CentralDirectory_UncompressedSizeMatches(int length)
    {
        // arrange
        byte[] content = GetRandomBytes(length);

        // act
        byte[] zipBytes = await CreateZipPackageBytes(content, useZip64: false);

        // assert
        var centralDirPos = FindSignature(zipBytes, Zip32CentralDirectoryEntry.KnownSignature);
        Assert.True(centralDirPos >= 0);

        uint uncompressedSize = BitConverter.ToUInt32(zipBytes, centralDirPos + Zip32CentralDirectoryEntry.Offsets.UncompressedSizeOffset);
        Assert.Equal((uint)content.Length, uncompressedSize);
    }

    [Fact]
    public async Task Zip32_CentralDirectory_ExternalFileAttributesAreZero()
    {
        // arrange
        byte[] content = GetRandomBytes();

        // act
        byte[] zipBytes = await CreateZipPackageBytes(content, useZip64: false);

        // assert
        var centralDirPos = FindSignature(zipBytes, Zip32CentralDirectoryEntry.KnownSignature);
        Assert.True(centralDirPos >= 0);

        uint extAttributes = BitConverter.ToUInt32(zipBytes, centralDirPos + 38);
        Assert.Equal(0u, extAttributes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abcd")]
    public async Task Zip32_CentralDirectory_FileCommentMatches(string? comment)
    {
        // arrange
        byte[] content = GetRandomBytes();

        // act
        byte[] zipBytes = await CreateZipPackageBytes(content, comment: comment, useZip64: false);

        // assert
        var centralDirPos = FindSignature(zipBytes, Zip32CentralDirectoryEntry.KnownSignature);
        Assert.True(centralDirPos >= 0);

        ushort fileNameLength = BitConverter.ToUInt16(zipBytes, centralDirPos + 28);
        ushort extraLength = BitConverter.ToUInt16(zipBytes, centralDirPos + 30);
        ushort commentLength = BitConverter.ToUInt16(zipBytes, centralDirPos + 32);
        int commentOffset = centralDirPos + 46 + fileNameLength + extraLength;
        string actualComment = Encoding.UTF8.GetString(zipBytes, commentOffset, commentLength);

        Assert.Equal(comment, actualComment);
    }

    [Fact]
    public async Task Zip32_CentralDirectory_FileNameMatches()
    {
        // arrange
        string filename = "mylongfilename.txt";
        byte[] content = GetRandomBytes();

        // act
        byte[] zipBytes = await CreateZipPackageBytes(content, filename, useZip64: false);

        // assert
        var centralDirPos = FindSignature(zipBytes, Zip32CentralDirectoryEntry.KnownSignature);
        Assert.True(centralDirPos >= 0);

        ushort nameLength = BitConverter.ToUInt16(zipBytes, centralDirPos + 28);
        string actualName = Encoding.UTF8.GetString(zipBytes, centralDirPos + 46, nameLength);

        Assert.Equal(filename, actualName);
    }

    // --- Helpers ---

    private static async Task<byte[]> CreateZipPackageBytes(byte[] content, string filename = "zipped.txt", string? comment = null, bool useZip64 = false)
    {
        using var ms = new MemoryStream();

        await using (var zipWriter = ZipWriter.Create(ms, leaveOpen: true, useZip64: useZip64))
        await using (var entryStream = zipWriter.OpenEntryStream(filename, comment))
        {
            await entryStream.WriteAsync(content);
        }

        return ms.ToArray();
    }

    private static byte[] GetRandomBytes(int length = 100)
    {
        var buffer = new byte[length];
        RandomNumberGenerator.Fill(buffer);
        return buffer;
    }

    private static int FindSignature(byte[] data, uint signature)
    {
        byte[] sigBytes = BitConverter.GetBytes(signature);
        for (int i = 0; i <= data.Length - 4; i++)
        {
            if (data[i] == sigBytes[0] &&
                data[i + 1] == sigBytes[1] &&
                data[i + 2] == sigBytes[2] &&
                data[i + 3] == sigBytes[3])
            {
                return i;
            }
        }

        return -1;
    }
}
