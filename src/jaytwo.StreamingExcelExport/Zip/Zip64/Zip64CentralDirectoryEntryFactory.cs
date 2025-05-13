using System;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64CentralDirectoryEntryFactory : ICentralDirectoryEntryFactory
{
    public const uint SeeZip64ExtraFields = 0xFFFFFFFF;

    public static Zip64CentralDirectoryEntry CreateCentralDirectoryEntry(
        ushort compressionMethod,
        uint? crc32 = default,
        ulong? compressedSize = default,
        ulong? uncompressedSize = default,
        string? fileName = default,
        string? fileComment = default,
        ulong? localHeaderOffset = default)
    {
        var result = new Zip64CentralDirectoryEntry()
        {
            Signature = Zip64CentralDirectoryEntry.KnownSignature,
            VersionMadeBy = ZipConstants.Versions.Version45,
            VersionNeededToExtract = ZipConstants.Versions.Version45,
            GeneralPurposeBitFlag = ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows,
            CompressionMethod = compressionMethod,
            LastModTime = 0,
            LastModDate = 0,
            DiskNumberStart = 0,
            InternalFileAttributes = 0,
            ExternalFileAttributes = 0,
            Crc32 = crc32,
            CompressedSize = SeeZip64ExtraFields,
            UncompressedSize = SeeZip64ExtraFields,
            LocalHeaderOffset = SeeZip64ExtraFields,
            Zip64CompressedSize = compressedSize ?? 0,
            Zip64UncompressedSize = uncompressedSize ?? 0,
            Zip64LocalHeaderOffset = localHeaderOffset ?? 0,
        };

        result.FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        result.FileNameLength = (ushort)Encoding.UTF8.GetByteCount(fileName);

        fileComment ??= string.Empty;
        result.FileComment = fileComment;
        result.FileCommentLength = (ushort)Encoding.UTF8.GetByteCount(fileComment);

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
            compressedSize: (ulong)compressedSize,
            uncompressedSize: (ulong)uncompressedSize,
            fileName: fileName,
            fileComment: fileComment,
            localHeaderOffset: (ulong)localHeaderOffset);
}
