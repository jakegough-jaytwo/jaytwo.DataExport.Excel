namespace jaytwo.DataExport.Excel.Zip.Zip64;

internal class Zip64DataDescriptorFactory : IDataDescriptorFactory
{
    public static Zip64DataDescriptor CreateDataDescriptor(
        uint? crc32 = default,
        ulong? compressedSize = default,
        ulong? uncompressedSize = default)
    {
        return new Zip64DataDescriptor
        {
            Signature = Zip64DataDescriptor.KnownSignature,
            Crc32 = crc32,
            CompressedSize = compressedSize,
            UncompressedSize = uncompressedSize,
        };
    }

    IZipPart IDataDescriptorFactory.CreateDataDescriptor(uint crc32, long compressedSize, long uncompressedSize)
        => CreateDataDescriptor(
            crc32: crc32,
            compressedSize: (ulong)compressedSize,
            uncompressedSize: (ulong)uncompressedSize);
}
