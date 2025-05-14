using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using jaytwo.DataExport.Excel.Zip.Zip32;
using jaytwo.DataExport.Excel.Zip.Zip64;

namespace jaytwo.DataExport.Excel.Zip;

internal abstract class ZipWriter : IZipWriter
{
    private ICentralDirectoryEntryFactory _centralDirectoryEntryFactory;
    private IDataDescriptorFactory _dataDescriptorFactory;
    private ILocalFileHeaderFactory _localFileHeaderFactory;
    private List<IZipPart> _entries = new List<IZipPart>();

    private bool _leaveOpen;

    public ZipWriter(
        Stream outputStream,
        bool leaveOpen,
        string? comment,
        ICentralDirectoryEntryFactory centralDirectoryEntryFactory,
        IDataDescriptorFactory dataDescriptorFactory,
        ILocalFileHeaderFactory localFileHeaderFactory)
    {
        OutputStream = outputStream;
        _leaveOpen = leaveOpen;
        Comment = comment;
        _centralDirectoryEntryFactory = centralDirectoryEntryFactory;
        _dataDescriptorFactory = dataDescriptorFactory;
        _localFileHeaderFactory = localFileHeaderFactory;
    }

    internal Stream OutputStream { get; }

    protected string? Comment { get; }

    public static ZipWriter Create(Stream outputStream, bool leaveOpen = false, string? comment = null, bool useZip64 = true)
        => useZip64
            ? CreateZip64(outputStream, leaveOpen, comment)
            : CreateZip32(outputStream, leaveOpen, comment);

    public static ZipWriter CreateZip32(Stream outputStream, bool leaveOpen = false, string? comment = null)
        => new Zip32Writer(outputStream, leaveOpen, comment);

    public static ZipWriter CreateZip64(Stream outputStream, bool leaveOpen = false, string? comment = null)
        => new Zip64Writer(outputStream, leaveOpen, comment);

    public Stream OpenEntryStream(string fileName, string? comment = default)
        => OpenEntryStream(fileName, comment, ZipConstants.CompressionMethods.Deflate);

    public Stream OpenEntryStream(string fileName, string? comment, ushort compressionMethod)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(nameof(fileName), "File name cannot be null or empty.");
        }

        WriteToOutput(
            _localFileHeaderFactory.CreateLocalFileHeader(compressionMethod, fileName),
            startPosition: out var localHeaderOffset);

        return new ZipEntryStream(this, compressionMethod, localHeaderOffset, fileName, comment);
    }

    public void Dispose()
    {
        WriteZipCentralDirectory();
        OutputStream.Flush();

        if (!_leaveOpen)
        {
            OutputStream.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        WriteZipCentralDirectory();
        await OutputStream.FlushAsync();

        if (!_leaveOpen)
        {
            await OutputStream.DisposeAsync();
        }
    }

    internal void WriteDataDescriptor(uint crc32, long compressedSize, long uncompressedSize)
        => WriteToOutput(
            _dataDescriptorFactory.CreateDataDescriptor(
                crc32: crc32,
                compressedSize: compressedSize,
                uncompressedSize: uncompressedSize));

    internal void AddCentralDirectoryEntry(
        ushort compressionMethod,
        uint crc32,
        long compressedSize,
        long uncompressedSize,
        string fileName,
        string? fileComment,
        long localHeaderOffset)
        => _entries.Add(_centralDirectoryEntryFactory.CreateCentralDirectoryEntry(
            compressionMethod: compressionMethod,
            crc32: crc32,
            compressedSize: compressedSize,
            uncompressedSize: uncompressedSize,
            fileComment: fileComment,
            fileName: fileName,
            localHeaderOffset: localHeaderOffset));

    protected void WriteZipCentralDirectory()
    {
        var totalEntries = _entries.Count;

        var centralDirectoryOffset = OutputStream.Position;

        foreach (var entry in _entries)
        {
            entry.WriteTo(OutputStream);
        }

        var centralDirectoryEnd = OutputStream.Position;

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
        startPosition = OutputStream.Position;
        part.WriteTo(OutputStream);
        endPosition = OutputStream.Position;
        partLength = endPosition - startPosition;
    }
}
