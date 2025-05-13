using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Force.Crc32;
using jaytwo.HashSiphon;
using jaytwo.StreamingExcelExport.Zip.Zip32;
using jaytwo.StreamingExcelExport.Zip.Zip64;

namespace jaytwo.StreamingExcelExport.Zip;

internal abstract class ZipWriter : IZipWriter
{
    private ICentralDirectoryEntryFactory _centralDirectoryEntryFactory;
    private IDataDescriptorFactory _dataDescriptorFactory;
    private ILocalFileHeaderFactory _localFileHeaderFactory;
    private List<IZipPart> _entries = new List<IZipPart>();

    private Stream _outputStream;
    private bool _leaveOpen;

    public ZipWriter(
        Stream outputStream,
        bool leaveOpen,
        string? comment,
        ICentralDirectoryEntryFactory centralDirectoryEntryFactory,
        IDataDescriptorFactory dataDescriptorFactory,
        ILocalFileHeaderFactory localFileHeaderFactory)
    {
        _outputStream = outputStream;
        _leaveOpen = leaveOpen;
        Comment = comment;
        _centralDirectoryEntryFactory = centralDirectoryEntryFactory;
        _dataDescriptorFactory = dataDescriptorFactory;
        _localFileHeaderFactory = localFileHeaderFactory;
    }

    protected string? Comment { get; }

    public static ZipWriter Create(Stream outputStream, bool leaveOpen = false, string? comment = null, bool useZip64 = true)
        => useZip64
            ? CreateZip64(outputStream, leaveOpen, comment)
            : CreateZip32(outputStream, leaveOpen, comment);

    public static ZipWriter CreateZip32(Stream outputStream, bool leaveOpen = false, string? comment = null)
        => new Zip32Writer(outputStream, leaveOpen, comment);

    public static ZipWriter CreateZip64(Stream outputStream, bool leaveOpen = false, string? comment = null)
        => new Zip64Writer(outputStream, leaveOpen, comment);

    public async Task WriteZipEntryAsync(string fileName, string? comment, Func<Stream, Task> writeFileCallback)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(nameof(fileName), "File name cannot be null or empty.");
        }

        var useDeflateCompression = true;

        var compressionMethod = useDeflateCompression
            ? ZipConstants.CompressionMethods.Deflate
            : ZipConstants.CompressionMethods.NoCompression;

        var localHeaderOffset = (uint)_outputStream.Position;
        WriteToOutput(
            _localFileHeaderFactory.CreateLocalFileHeader(compressionMethod, fileName));

        uint crc32 = 0;
        long uncompressedSize = 0;
        var startPosition = _outputStream.Position;

        Stream compressionStream = useDeflateCompression
            ? new DeflateStream(_outputStream, CompressionMode.Compress, leaveOpen: true)
            : new PassthroughStream(_outputStream, leaveOpen: true);

        using (compressionStream)
        using (var hashSiphon = HashSiphonStream.CreateWrite(compressionStream, () => new Crc32Algorithm(isBigEndian: false), leaveOpen: true))
        {
            await writeFileCallback(hashSiphon);
            await hashSiphon.FlushAsync(finalizeHash: true);

            crc32 = BitConverter.ToUInt32(hashSiphon.Hash);
            uncompressedSize = hashSiphon.BytesWritten;
        }

        var endPosition = _outputStream.Position;
        var compressedSize = endPosition - startPosition;

        WriteToOutput(
            _dataDescriptorFactory.CreateDataDescriptor(
                crc32: crc32,
                compressedSize: compressedSize,
                uncompressedSize: uncompressedSize));

        _entries.Add(
            _centralDirectoryEntryFactory.CreateCentralDirectoryEntry(
                compressionMethod: compressionMethod,
                crc32: crc32,
                compressedSize: compressedSize,
                uncompressedSize: uncompressedSize,
                fileComment: comment,
                fileName: fileName,
                localHeaderOffset: localHeaderOffset));
    }

    public void Dispose()
    {
        WriteZipCentralDirectory();
        _outputStream.Flush();

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

    protected void WriteZipCentralDirectory()
    {
        var totalEntries = _entries.Count;

        var centralDirectoryOffset = _outputStream.Position;

        foreach (var entry in _entries)
        {
            entry.WriteTo(_outputStream);
        }

        var centralDirectoryEnd = _outputStream.Position;

        var centralDirectorySize = centralDirectoryEnd - centralDirectoryOffset;

        WriteZipEndOfCentralDirectory(
            totalEntries: totalEntries,
            centralDirectorySize: centralDirectorySize,
            centralDirectoryOffset: centralDirectoryOffset,
            comment: Comment);
    }

    protected abstract void WriteZipEndOfCentralDirectory(int totalEntries, long centralDirectorySize, long centralDirectoryOffset, string? comment);

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
