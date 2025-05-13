namespace jaytwo.StreamingExcelExport.Zip;

internal interface ICentralDirectoryEntryFactory
{
    IZipPart CreateCentralDirectoryEntry(
        ushort compressionMethod,
        uint crc32,
        long compressedSize,
        long uncompressedSize,
        string fileName,
        string? fileComment,
        long localHeaderOffset);
}
