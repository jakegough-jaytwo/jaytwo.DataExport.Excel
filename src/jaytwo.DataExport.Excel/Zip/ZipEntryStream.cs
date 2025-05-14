using System;
using System.IO;
using System.IO.Compression;
using Force.Crc32;
using jaytwo.HashSiphon;

namespace jaytwo.DataExport.Excel.Zip;

internal class ZipEntryStream : Stream
{
    private readonly ZipWriter _zipWriter;
    private readonly Stream _compressedStream;
    private readonly HashSiphonStream _hashSiphon;

    public ZipEntryStream(ZipWriter zipWriter, ushort compressionMethod, long localHeaderOffset, string fileName, string? comment)
    {
        _zipWriter = zipWriter ?? throw new ArgumentNullException(nameof(zipWriter));

        FileName = fileName;

        CompressionMethod = compressionMethod;

        LocalHeaderOffset = localHeaderOffset;

        Comment = comment;

        OutputStreamStartPosition = _zipWriter.OutputStream.Position;

        _compressedStream = CompressionMethod == ZipConstants.CompressionMethods.Deflate
            ? new DeflateStream(_zipWriter.OutputStream, CompressionMode.Compress, leaveOpen: true)
            : new PassthroughStream(_zipWriter.OutputStream, leaveOpen: true);

        _hashSiphon = HashSiphonStream.CreateWrite(
            _compressedStream,
            () => new Crc32Algorithm(isBigEndian: false),
            leaveOpen: true);
    }

    public string FileName { get; }

    public string? Comment { get; }

    public ushort CompressionMethod { get; }

    public long LocalHeaderOffset { get; }

    public long OutputStreamStartPosition { get; }

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => _hashSiphon.CanWrite;

    public override long Length => _hashSiphon.Length;

    public override long Position { get => _hashSiphon.Position; set => throw new NotSupportedException(); }

    public override void Flush() => _hashSiphon.Flush();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => _hashSiphon.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        _hashSiphon.Dispose();
        _compressedStream.Dispose();

        var crc32 = BitConverter.ToUInt32(_hashSiphon.Hash);
        var uncompressedSize = _hashSiphon.BytesWritten;

        var endPosition = _zipWriter.OutputStream.Position;
        var compressedSize = endPosition - OutputStreamStartPosition;

        _zipWriter.WriteDataDescriptor(
            crc32: crc32,
            compressedSize: compressedSize,
            uncompressedSize: uncompressedSize);

        _zipWriter.AddCentralDirectoryEntry(
            compressionMethod: CompressionMethod,
            crc32: crc32,
            compressedSize: compressedSize,
            uncompressedSize: uncompressedSize,
            fileComment: Comment,
            fileName: FileName,
            localHeaderOffset: LocalHeaderOffset);
    }
}
