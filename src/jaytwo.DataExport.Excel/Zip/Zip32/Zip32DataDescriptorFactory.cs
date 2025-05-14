namespace jaytwo.DataExport.Excel.Zip.Zip32;

internal class Zip32DataDescriptorFactory : IDataDescriptorFactory
{
    public static Zip32DataDescriptor CreateDataDescriptor(
        uint? crc32 = default,
        uint? compressedSize = default,
        uint? uncompressedSize = default)
    {
        return new Zip32DataDescriptor
        {
            Signature = Zip32DataDescriptor.KnownSignature,
            Crc32 = crc32,
            CompressedSize = compressedSize,
            UncompressedSize = uncompressedSize,
        };
    }

    IZipPart IDataDescriptorFactory.CreateDataDescriptor(uint crc32, long compressedSize, long uncompressedSize)
        => CreateDataDescriptor(
            crc32: crc32,
            compressedSize: (uint)compressedSize,
            uncompressedSize: (uint)uncompressedSize);
}
