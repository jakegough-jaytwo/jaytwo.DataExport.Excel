using System.Text;

namespace jaytwo.DataExport.Excel.Zip.Zip64;

internal class Zip64LocalFileHeaderFactory : ILocalFileHeaderFactory
{
    public const uint SeeZip64ExtraFields = 0xFFFFFFFF;

    public static Zip64LocalFileHeader CreateLocalFileHeader(ushort compressionMethod, string fileName)
    {
        var result = new Zip64LocalFileHeader
        {
            Signature = Zip64LocalFileHeader.KnownSignature,
            VersionNeededToExtract = ZipConstants.Versions.Version45,
            GeneralPurposeBitFlag = ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows,
            CompressionMethod = compressionMethod,
            LastModTime = 0,
            LastModDate = 0,
            Crc32 = 0,
            CompressedSize = SeeZip64ExtraFields,
            UncompressedSize = SeeZip64ExtraFields,
            FileName = fileName,
            FileNameLength = (ushort)Encoding.UTF8.GetByteCount(fileName ?? string.Empty),
            Zip64CompressedSize = 0,
            Zip64UncompressedSize = 0,
        };

        return result;
    }

    IZipPart ILocalFileHeaderFactory.CreateLocalFileHeader(ushort compressionMethod, string fileName)
        => CreateLocalFileHeader(compressionMethod, fileName);
}
