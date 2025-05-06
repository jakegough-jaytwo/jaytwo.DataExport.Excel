using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Force.Crc32;
using jaytwo.HashSiphon;

namespace jaytwo.StreamingExcelExport;

internal class ZipWriter : IDisposable, IAsyncDisposable
{
    private List<ZipCentralDirectoryEntry> _entries = new List<ZipCentralDirectoryEntry>();

    private Stream _outputStream;
    private bool _leaveOpen;

    public ZipWriter(Stream outputStream, bool leaveOpen = false)
    {
        _outputStream = outputStream;
        _leaveOpen = leaveOpen;
    }

    public async Task WriteFileAsync(string fileName, string comment, Func<Stream, Task> writeFileCallback)
    {
        var localHeaderOffset = (uint)_outputStream.Position;
        new ZipLocalFileHeader { FileName = fileName }.WriteTo(_outputStream);

        uint crc32 = 0;
        var startPosition = _outputStream.Position;
        using (var hashSiphon = HashSiphonStream.CreateWrite(_outputStream, () => new Crc32Algorithm(), leaveInnerStreamOpen: true))
        {
            await writeFileCallback(hashSiphon);
            hashSiphon.Flush(finalizeHash: true);
            crc32 = BitConverter.ToUInt32(hashSiphon.Hash);
        }

        var endPosition = _outputStream.Position;
        var fileLength = (uint)(endPosition - startPosition);

        new ZipDataDescriptor { Crc32 = crc32, UncompressedSize = fileLength, }.WriteTo(_outputStream);

        _entries.Add(new ZipCentralDirectoryEntry
        {
            FileName = fileName,
            UncompressedSize = fileLength,
            FileComment = comment,
            Crc32 = crc32,
            LocalHeaderOffset = localHeaderOffset,
        });
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

    private void WriteZipCentralDirectory()
    {
        var centralDirectoryStart = _outputStream.Position;

        foreach (var entry in _entries)
        {
            entry.WriteTo(_outputStream);
        }

        var centralDirectoryEnd = _outputStream.Position;
        uint centralDirectorySize = (uint)(centralDirectoryEnd - centralDirectoryStart);

        new ZipEndOfCentralDirectory
        {
            TotalEntries = (ushort)_entries.Count,
            CentralDirectoryOffset = (uint)centralDirectoryStart,
            CentralDirectorySize = centralDirectorySize,
            Comment = string.Empty,
        }
        .WriteTo(_outputStream);
    }

    public class ZipLocalFileHeader
    {
        private const uint Signature = 0x04034b50;

        private const uint EmptyCentralDirectorySize = 30;

        public ushort VersionNeededToExtract => 20; // ZIP 2.0

        public ushort GeneralPurposeBitFlag => 0x08; // bit 3 = data descriptor follows

        public ushort CompressionMethod => 0; // 0 = Store (no compression)

        public ushort LastModTime => 0;

        public ushort LastModDate => 0;

        public uint Crc32Placeholder => 0;

        public uint CompressedSizePlaceholder => 0;

        public uint UncompressedSizePlaceholder => 0;

        public string FileName { get; set; } = string.Empty;

        public void WriteTo(Stream stream)
        {
            if (stream == null || !stream.CanWrite)
            {
                throw new ArgumentException("Stream must be writable.", nameof(stream));
            }

            var fileNameBytes = Encoding.UTF8.GetBytes(FileName);

            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

            writer.Write(Signature);                          // 0x04034b50
            writer.Write(VersionNeededToExtract);             // version needed
            writer.Write(GeneralPurposeBitFlag);              // flags (bit 3 = data descriptor)
            writer.Write(CompressionMethod);                  // method (0 = store)
            writer.Write(LastModTime);                        // time
            writer.Write(LastModDate);                        // date
            writer.Write(Crc32Placeholder);                   // placeholder
            writer.Write(CompressedSizePlaceholder);          // placeholder
            writer.Write(UncompressedSizePlaceholder);        // placeholder
            writer.Write((ushort)fileNameBytes.Length);       // file name length
            writer.Write((ushort)0);                          // extra field length
            writer.Write(fileNameBytes);                      // file name
        }

        public uint GetTotalLength()
        {
            var fileNameLength = (uint)Encoding.UTF8.GetByteCount(FileName);
            return EmptyCentralDirectorySize + fileNameLength;
        }
    }

    public class ZipDataDescriptor
    {
        private const uint Signature = 0x08074b50; // Recommended for compatibility

        public uint Crc32 { get; set; }

        public uint CompressedSize => UncompressedSize;

        public uint UncompressedSize { get; set; }

        public void WriteTo(Stream stream)
        {
            if (stream == null || !stream.CanWrite)
            {
                throw new ArgumentException("Stream must be writable.", nameof(stream));
            }

            using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);

            writer.Write(Signature);          // 0x08074b50
            writer.Write(Crc32);              // CRC-32 of uncompressed data
            writer.Write(CompressedSize);     // Compressed size (same as uncompressed for method 0)
            writer.Write(UncompressedSize);   // Uncompressed size
        }

        public int GetTotalLength() => 16;
    }

    public class ZipCentralDirectoryEntry
    {
        private const uint Signature = 0x02014b50;

        private const uint EmptyCentralDirectorySize = 46;

        public ushort VersionMadeBy => 20; // 2.0

        public ushort VersionNeededToExtract => VersionMadeBy;

        public ushort GeneralPurposeBitFlag => 0x08; // bit 3: data descriptor follows

        public ushort CompressionMethod => 0; // 0 = Store

        public ushort LastModTime => 0;

        public ushort LastModDate => 0;

        public uint Crc32 { get; set; }

        public uint CompressedSize => UncompressedSize;

        public uint UncompressedSize { get; set; }

        public string FileName { get; set; } = string.Empty;

        public ushort DiskNumberStart => 0;

        public ushort InternalFileAttributes => 0;

        public uint ExternalFileAttributes => 0;

        public uint LocalHeaderOffset { get; set; }

        public string? FileComment { get; set; }

        public void WriteTo(Stream stream)
        {
            if (stream == null || !stream.CanWrite)
            {
                throw new ArgumentException("Stream must be writable.", nameof(stream));
            }

            var fileNameBytes = Encoding.UTF8.GetBytes(FileName);
            var commentBytes = Encoding.UTF8.GetBytes(FileComment ?? string.Empty);
            var length = EmptyCentralDirectorySize + fileNameBytes.Length + commentBytes.Length;

            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            writer.Write(Signature);
            writer.Write(VersionMadeBy);
            writer.Write(VersionNeededToExtract);
            writer.Write(GeneralPurposeBitFlag);
            writer.Write(CompressionMethod);
            writer.Write(LastModTime);
            writer.Write(LastModDate);
            writer.Write(Crc32);
            writer.Write(CompressedSize);
            writer.Write(UncompressedSize);
            writer.Write((ushort)fileNameBytes.Length);
            writer.Write((ushort)0); // extra field length
            writer.Write((ushort)commentBytes.Length);
            writer.Write(DiskNumberStart);
            writer.Write(InternalFileAttributes);
            writer.Write(ExternalFileAttributes);
            writer.Write(LocalHeaderOffset);
            writer.Write(fileNameBytes);
            writer.Write(commentBytes);
        }

        public uint GetTotalLength()
        {
            var fileNameLength = (uint)Encoding.UTF8.GetByteCount(FileName);
            var commentLength = (uint)Encoding.UTF8.GetByteCount(FileComment ?? string.Empty);
            return EmptyCentralDirectorySize + fileNameLength + commentLength;
        }
    }

    public class ZipEndOfCentralDirectory
    {
        private const uint Signature = 0x06054b50;

        private const uint EmptyEndOfCentralDirectorySize = 22;

        public ushort DiskNumber { get; } = 0;

        public ushort CentralDirectoryStartDisk { get; } = 0;

        public ushort TotalEntriesOnThisDisk => TotalEntries;

        public ushort TotalEntries { get; set; }

        public uint CentralDirectoryOffset { get; set; }

        public uint CentralDirectorySize { get; set; }

        public string? Comment { get; set; }

        public void WriteTo(Stream stream)
        {
            if (stream == null || !stream.CanWrite)
            {
                throw new ArgumentException("Stream must be writable.", nameof(stream));
            }

            var commentBytes = Encoding.UTF8.GetBytes(Comment ?? string.Empty);

            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            writer.Write(Signature);                                 // 0x06054b50
            writer.Write(DiskNumber);                                // Disk number
            writer.Write(CentralDirectoryStartDisk);                 // Start disk
            writer.Write(TotalEntriesOnThisDisk);                    // # entries on this disk
            writer.Write(TotalEntries);                              // Total entries
            writer.Write(CentralDirectorySize);                      // Size of central dir
            writer.Write(CentralDirectoryOffset);                    // Offset of central dir
            writer.Write((ushort)commentBytes.Length);               // Comment length
            writer.Write(commentBytes);                              // Comment
        }
    }
}
