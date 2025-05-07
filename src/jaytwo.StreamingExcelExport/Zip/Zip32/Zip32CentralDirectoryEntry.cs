using System;
using System.IO;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32CentralDirectoryEntry : IZipPart
{
    public const uint Signature = 0x02014b50;

    private const uint EmptyCentralDirectorySize = 30;

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

    public uint CompressedSize => UncompressedSize;

    public uint UncompressedSize { get; set; }

    public string FileName { get; set; } = string.Empty;

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
}
