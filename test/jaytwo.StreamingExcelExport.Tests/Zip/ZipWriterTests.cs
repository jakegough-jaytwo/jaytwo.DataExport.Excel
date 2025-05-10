using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jaytwo.StreamingExcelExport.Zip;
using Xunit;

namespace jaytwo.StreamingExcelExport.Tests.Zip;

public class ZipWriterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ZipWriter_CreatesValidZipFile(bool useZip64)
    {
        using var ms = new MemoryStream();
        var zipWriter = ZipWriter.Create(ms, leaveOpen: true, useZip64: useZip64);

        await zipWriter.WriteFileAsync("hello.txt", "optional comment", async stream =>
        {
            var content = Encoding.UTF8.GetBytes("Hello, ZIP!");
            await stream.WriteAsync(content);
        });

        await zipWriter.DisposeAsync();
        ms.Position = 0;

        using var zip = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: false);
        var entry = zip.Entries.Single();
        Assert.Equal("hello.txt", entry.FullName);

        using var reader = new StreamReader(entry.Open());
        var text = await reader.ReadToEndAsync();
        Assert.Equal("Hello, ZIP!", text);
    }

    [Theory]
    [InlineData(256)]
    [InlineData(65536)]
    [InlineData(1000000)]
    [InlineData(2000000)]
    [InlineData(4000000)]
    [InlineData(8000000)]
    //[InlineData(16000000)]
    //[InlineData(32000000)]
    public async Task ZipWriter_VerifyEntryRoundTrip(int length)
    {
        using var ms = new MemoryStream();
        var bytes = Enumerable.Range(0, length).Select(i => (byte)(i % 256)).ToArray();
        using (var zipWriter = ZipWriter.CreateZip32(ms, leaveOpen: true))
        {
            await zipWriter.WriteFileAsync("data.bin", string.Empty, async stream =>
            {
                await stream.WriteAsync(bytes);
            });
        }

        ms.Position = 0;

        using var zip = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: false);
        var entry = zip.GetEntry("data.bin")!;

        using var zipStream = entry.Open();
        var buffer = new byte[entry.Length];
        var read = await zipStream.ReadAsync(buffer, 0, buffer.Length);

        Assert.Equal(bytes, buffer);
    }

    [Theory]
    [InlineData(1024 * 1024 * 70)]
    public async Task ZipWriter_WritesLargeFileCorrectly(int fileSize)
    {
        using var ms = new MemoryStream();
        var zipWriter = ZipWriter.CreateZip64(ms, leaveOpen: true);

        await zipWriter.WriteFileAsync("large.dat", string.Empty, async stream =>
        {
            var buffer = new byte[8192];
            var total = 0;
            while (total < fileSize)
            {
                await stream.WriteAsync(buffer, 0, Math.Min(buffer.Length, fileSize - total));
                total += buffer.Length;
            }
        });

        await zipWriter.DisposeAsync();
        ms.Position = 0;

        using var zip = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: false);
        var entry = zip.GetEntry("large.dat")!;
        Assert.Equal(fileSize, entry.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(25)]
    public async Task ZipWriter_WritesMultipleFilesCorrectly(int fileCount)
    {
        using var ms = new MemoryStream();
        var zipWriter = ZipWriter.CreateZip32(ms, leaveOpen: true);

        var expectedFiles = new (string Name, string Content)[fileCount];
        for (int i = 0; i < fileCount; i++)
        {
            var name = $"file{i + 1}.txt";
            var content = $"This is the content of file {i + 1}";
            expectedFiles[i] = (name, content);

            await zipWriter.WriteFileAsync(name, string.Empty, async stream =>
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                await stream.WriteAsync(bytes);
            });
        }

        await zipWriter.DisposeAsync();
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
