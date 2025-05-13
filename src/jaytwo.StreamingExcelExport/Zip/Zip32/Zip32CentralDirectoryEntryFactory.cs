using System;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32CentralDirectoryEntryFactory : ICentralDirectoryEntryFactory
{
    public static Zip32CentralDirectoryEntry CreateCentralDirectoryEntry(
        ushort compressionMethod,
        uint? crc32 = default,
        uint? compressedSize = default,
        uint? uncompressedSize = default,
        string? fileName = default,
        string? fileComment = default,
        uint? localHeaderOffset = default,
        byte[]? extraField = default)
    {
        var result = new Zip32CentralDirectoryEntry()
        {
            Signature = Zip32CentralDirectoryEntry.KnownSignature,
            VersionMadeBy = ZipConstants.Versions.Version20,
            VersionNeededToExtract = ZipConstants.Versions.Version20,
            GeneralPurposeBitFlag = ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows,
            CompressionMethod = compressionMethod,
            LastModTime = 0,
            LastModDate = 0,
            DiskNumberStart = 0,
            InternalFileAttributes = 0,
            ExternalFileAttributes = 0,
            Crc32 = crc32,
            CompressedSize = compressedSize,
            UncompressedSize = uncompressedSize,
            ExtraFieldLength = 0,
            ExtraField = Array.Empty<byte>(),
        };

        fileName ??= string.Empty;
        result.FileName = fileName;
        result.FileNameLength = (ushort)Encoding.UTF8.GetByteCount(fileName);

        fileComment ??= string.Empty;
        result.FileComment = fileComment;
        result.FileCommentLength = (ushort)Encoding.UTF8.GetByteCount(fileComment);

        extraField ??= Array.Empty<byte>();
        result.ExtraField = extraField;
        result.ExtraFieldLength = (ushort)extraField.Length;

        result.LocalHeaderOffset = localHeaderOffset;

        return result;
    }

    IZipPart ICentralDirectoryEntryFactory.CreateCentralDirectoryEntry(
        ushort compressionMethod,
        uint crc32,
        long compressedSize,
        long uncompressedSize,
        string fileName,
        string? fileComment,
        long localHeaderOffset)
        => CreateCentralDirectoryEntry(
            compressionMethod: compressionMethod,
            crc32: crc32,
            compressedSize: (uint)compressedSize,
            uncompressedSize: (uint)uncompressedSize,
            fileName: fileName,
            fileComment: fileComment,
            localHeaderOffset: (uint)localHeaderOffset);
}
