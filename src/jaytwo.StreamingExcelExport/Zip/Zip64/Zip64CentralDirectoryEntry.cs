using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using jaytwo.StreamingExcelExport.Zip.Zip32;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64CentralDirectoryEntry : IZipPart
{
    public const uint Signature = Zip32CentralDirectoryEntry.Signature;
    private const ushort Zip64ExtraFieldHeaderId = 0x0001;
    private const uint SeeZip64ExtraFields = 0xFFFFFFFF;
    private const ushort VersionMadeBy = ZipConstants.Versions.Version20;
    private const ushort VersionNeededToExtract = VersionMadeBy;
    private const ushort GeneralPurposeBitFlag = ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows;
    private const ushort CompressionMethod = ZipConstants.CompressionMethods.NoCompression;
    private const ushort LastModTime = 0;
    private const ushort LastModDate = 0;
    private const ushort DiskNumberStart = 0;
    private const ushort InternalFileAttributes = 0;
    private const uint ExternalFileAttributes = 0;

    public uint Crc32 { get; set; }

    public ulong CompressedSize => UncompressedSize;

    public ulong UncompressedSize { get; set; }

    public string FileName { get; set; } = string.Empty;

    public ulong LocalHeaderOffset { get; set; }

    public string? FileComment { get; set; }

    public void WriteTo(Stream stream)
    {
        if (stream == null || !stream.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(stream));
        }

        var fileNameBytes = Encoding.UTF8.GetBytes(FileName);
        var commentBytes = Encoding.UTF8.GetBytes(FileComment ?? string.Empty);

        var extraFieldHeaderLength = 4; // 2 byte header id + 2 byte data size
        var extraFieldDataLength = 24; // 8 byte UncompressedSize + 8 byte CompressedSize + 8 byte LocalHeaderOffset
        var extraFieldTotalLength = extraFieldHeaderLength + extraFieldDataLength;

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(Signature);
        writer.Write(VersionMadeBy);
        writer.Write(VersionNeededToExtract);
        writer.Write(GeneralPurposeBitFlag);
        writer.Write(CompressionMethod);
        writer.Write(LastModTime);
        writer.Write(LastModDate);
        writer.Write(Crc32);
        writer.Write(SeeZip64ExtraFields); // CompressedSize
        writer.Write(SeeZip64ExtraFields); // UncompressedSize
        writer.Write((ushort)fileNameBytes.Length);
        writer.Write((ushort)extraFieldTotalLength);
        writer.Write((ushort)commentBytes.Length);
        writer.Write(DiskNumberStart);
        writer.Write(InternalFileAttributes);
        writer.Write(ExternalFileAttributes);
        writer.Write(SeeZip64ExtraFields); // LocalHeaderOffset
        writer.Write(fileNameBytes);

        // Write ZIP64 extra field
        writer.Write(Zip64ExtraFieldHeaderId); // ID = 0x0001
        writer.Write((ushort)extraFieldDataLength);
        writer.Write(UncompressedSize);
        writer.Write(CompressedSize);
        writer.Write(LocalHeaderOffset);

        writer.Write(commentBytes);
    }
}
