using System;
using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32LocalFileHeaderFactory : ILocalFileHeaderFactory
{
    public static Zip32LocalFileHeader CreateLocalFileHeader(ushort compressionMethod, string fileName)
    {
        return new Zip32LocalFileHeader
        {
            Signature = Zip32LocalFileHeader.KnownSignature,
            VersionNeededToExtract = ZipConstants.Versions.Version20,
            GeneralPurposeBitFlag = ZipConstants.GeneralPurposeBitFlags.DataDescriptorFollows,
            CompressionMethod = compressionMethod,
            LastModTime = 0,
            LastModDate = 0,
            Crc32 = 0,
            CompressedSize = 0,
            UncompressedSize = 0,
            FileName = fileName,
            FileNameLength = (ushort)Encoding.UTF8.GetByteCount(fileName ?? string.Empty),
            ExtraField = Array.Empty<byte>(),
            ExtraFieldLength = 0,
        };
    }

    IZipPart ILocalFileHeaderFactory.CreateLocalFileHeader(ushort compressionMethod, string fileName)
        => CreateLocalFileHeader(compressionMethod, fileName);
}
