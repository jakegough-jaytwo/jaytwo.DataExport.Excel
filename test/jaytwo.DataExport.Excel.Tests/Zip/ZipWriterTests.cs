using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaytwo.DataExport.Excel.Zip;
using Xunit;

namespace jaytwo.DataExport.Excel.Tests.Zip;

public class ZipWriterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ZipWriter_CreatesValidZipFile(bool useZip64)
    {
        // arrange
        using var ms = new MemoryStream();
        var content = Encoding.UTF8.GetBytes("Hello, ZIP!");

        // act
        await using (var zipWriter = ZipWriter.Create(ms, leaveOpen: true, useZip64: useZip64))
        await using (var entryStream = zipWriter.OpenEntryStream("hello.txt", "optional comment"))
        {
            await entryStream.WriteAsync(content);
        }

        // assert
        ms.Position = 0;

        using var zip = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: false);
        var entry = zip.Entries.Single();
        Assert.Equal("hello.txt", entry.FullName);

        using var reader = new StreamReader(entry.Open());
        var text = await reader.ReadToEndAsync();
        Assert.Equal("Hello, ZIP!", text);
    }

    [Theory]
    [InlineData(true, 256)]
    [InlineData(true, 65536)]
    [InlineData(true, 1000000)]
    [InlineData(true, 2000000)]
    [InlineData(true, 4000000)]
    [InlineData(true, 8000000)]
    [InlineData(true, 16000000)]
    [InlineData(true, 32000000)]
    [InlineData(false, 256)]
    [InlineData(false, 65536)]
    [InlineData(false, 1000000)]
    [InlineData(false, 2000000)]
    [InlineData(false, 4000000)]
    [InlineData(false, 8000000)]
    [InlineData(false, 16000000)]
    [InlineData(false, 32000000)]
    public async Task ZipWriter_VerifyEntryRoundTrip(bool useZip64, int length)
    {
        // arrange
        using var ms = new MemoryStream();
        var bytes = Enumerable.Range(0, length).Select(i => (byte)(i % 256)).ToArray();
        var expectedCrc = Crc32Helper.ComputeCrc(bytes);

        // act
        await using (var zipWriter = ZipWriter.Create(ms, leaveOpen: true, useZip64: useZip64))
        await using (var entryStream = zipWriter.OpenEntryStream("data.bin"))
        {
            await entryStream.WriteAsync(bytes);
        }

        // assert
        ms.Position = 0;

        using var zip = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: false);
        var entry = zip.GetEntry("data.bin")!;

        using var zipStream = entry.Open();
        var zipStreamCrc = Crc32Helper.ComputeCrc(zipStream);
        Assert.Equal(expectedCrc, zipStreamCrc);
    }

    [Theory]
    [InlineData(true, 1024 * 1024 * 70)]
    [InlineData(false, 1024 * 1024 * 70)]
    public async Task ZipWriter_WritesLargeFileCorrectly(bool useZip64, int fileSize)
    {
        // arrange
        using var ms = new MemoryStream();

        // act
        await using (var zipWriter = ZipWriter.Create(ms, leaveOpen: true, useZip64: useZip64))
        await using (var entryStream = zipWriter.OpenEntryStream("large.dat"))
        {
            var buffer = new byte[8192];
            var total = 0;
            while (total < fileSize)
            {
                await entryStream.WriteAsync(buffer, 0, Math.Min(buffer.Length, fileSize - total));
                total += buffer.Length;
            }
        }

        // assert
        ms.Position = 0;

        using var zip = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: false);
        var entry = zip.GetEntry("large.dat")!;
        Assert.Equal(fileSize, entry.Length);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(true, 5)]
    [InlineData(true, 25)]
    [InlineData(false, 1)]
    [InlineData(false, 5)]
    [InlineData(false, 25)]
    public async Task ZipWriter_WritesMultipleFilesCorrectly(bool useZip64, int fileCount)
    {
        // arrange
        using var ms = new MemoryStream();
        var expectedFiles = new (string Name, string Content)[fileCount];

        // act
        await using (var zipWriter = ZipWriter.Create(ms, leaveOpen: true, useZip64: useZip64))
        {
            for (int i = 0; i < fileCount; i++)
            {
                var name = $"file{i + 1}.txt";
                var content = $"This is the content of file {i + 1}";
                expectedFiles[i] = (name, content);

                await using (var entryStream = zipWriter.OpenEntryStream(name))
                {
                    var bytes = Encoding.UTF8.GetBytes(content);
                    await entryStream.WriteAsync(bytes);
                }
            }
        }

        // assert
        ms.Position = 0;

        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.Equal(fileCount, zip.Entries.Count);

        foreach (var (expectedName, expectedContent) in expectedFiles)
        {
            var entry = zip.GetEntry(expectedName)!;
            Assert.NotNull(entry);

            using var reader = new StreamReader(entry.Open());
            var actualContent = await reader.ReadToEndAsync();
            Assert.Equal(expectedContent, actualContent);
        }
    }
}
