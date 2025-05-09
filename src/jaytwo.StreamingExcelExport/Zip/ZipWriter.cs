using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Force.Crc32;
using jaytwo.HashSiphon;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using jaytwo.StreamingExcelExport.Zip.Zip64;

namespace jaytwo.StreamingExcelExport.Zip;

internal abstract class ZipWriter : IZipWriter
{
    private List<IZipPart> _entries = new List<IZipPart>();

    private Stream _outputStream;
    private bool _leaveOpen;

    public ZipWriter(Stream outputStream, bool leaveOpen = false)
    {
        _outputStream = outputStream;
        _leaveOpen = leaveOpen;
    }

    public static ZipWriter Create(Stream outputStream, bool leaveOpen = false, bool useZip64 = true)
        => useZip64
            ? CreateZip64(outputStream, leaveOpen)
            : CreateZip32(outputStream, leaveOpen);

    public static ZipWriter CreateZip32(Stream outputStream, bool leaveOpen = false)
        => new Zip32Writer(outputStream, leaveOpen);

    public static ZipWriter CreateZip64(Stream outputStream, bool leaveOpen = false)
        => new Zip64Writer(outputStream, leaveOpen);

    public async Task WriteFileAsync(string fileName, string? comment, Func<Stream, Task> writeFileCallback)
    {
        var localHeaderOffset = (uint)_outputStream.Position;
        WriteLocalFileHeader(fileName);

        uint crc32 = 0;
        var startPosition = _outputStream.Position;
        using (var hashSiphon = HashSiphonStream.CreateWrite(_outputStream, () => new Crc32Algorithm(), leaveInnerStreamOpen: true))
        {
            await writeFileCallback(hashSiphon);
            hashSiphon.Flush(finalizeHash: true);
            crc32 = BitConverter.ToUInt32(hashSiphon.Hash!.Reverse().ToArray()); // i have no idea why the bytes have to be reversed here
        }

        var endPosition = _outputStream.Position;
        var fileLength = endPosition - startPosition;

        WriteDataDescriptor(crc32, fileLength, fileLength);

        _entries.Add(BuildCentralDirectoryEntry(
            ZipConstants.CompressionMethods.NoCompression,
            fileName,
            fileLength,
            fileLength,
            comment,
            crc32,
            localHeaderOffset));
    }

    public void Dispose()
    {
        WriteZipCentralDirectory();
        _outputStream.FlushAsync();

        if (!_leaveOpen)
        {
            _outputStream.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        WriteZipCentralDirectory();
        await _outputStream.FlushAsync();

        if (!_leaveOpen)
        {
            await _outputStream.DisposeAsync();
        }
    }

    protected abstract IZipPart BuildLocalFileHeader(string fileName);

    protected void WriteLocalFileHeader(string fileName)
        => WriteToOutput(BuildLocalFileHeader(fileName));

    protected abstract IZipPart BuildDataDescriptor(uint crc32, long compressedSize, long uncompressedSize);

    protected void WriteDataDescriptor(uint crc32, long compressedSize, long uncompressedSize)
        => WriteToOutput(BuildDataDescriptor(crc32, compressedSize, uncompressedSize));

    protected abstract IZipPart BuildCentralDirectoryEntry(
        ushort compressionMethod,
        string fileName,
        long compressedSize,
        long uncompressedSize,
        string? comment,
        uint crc32,
        long localHeaderOffset);

    protected void WriteZip32EndOfCentralDirectory(int totalEntries, long centralDirectoryOffset, long centralDirectorySize, string? comment)
        => WriteToOutput(new Zip32EndOfCentralDirectory
        {
            TotalEntries = (ushort)totalEntries,
            CentralDirectoryOffset = (uint)centralDirectoryOffset,
            CentralDirectorySize = (uint)centralDirectorySize,
            Comment = comment,
        });

    protected void WriteZipCentralDirectoryEntries(out int totalEntries, out long startPosition, out long endPosition)
    {
        totalEntries = _entries.Count;

        startPosition = _outputStream.Position;

        foreach (var entry in _entries)
        {
            entry.WriteTo(_outputStream);
        }

        endPosition = _outputStream.Position;
    }

    protected abstract void WriteZipCentralDirectory();

    protected void WriteToOutput(IZipPart part)
        => WriteToOutput(part, out _, out _, out _);

    protected void WriteToOutput(IZipPart part, out long startPosition)
        => WriteToOutput(part, out startPosition, out _, out _);

    protected void WriteToOutput(IZipPart part, out long startPosition, out long endPosition)
        => WriteToOutput(part, out startPosition, out endPosition, out _);

    protected void WriteToOutput(IZipPart part, out long startPosition, out long endPosition, out long partLength)
    {
        startPosition = _outputStream.Position;
        part.WriteTo(_outputStream);
        endPosition = _outputStream.Position;
        partLength = endPosition - startPosition;
    }
}
