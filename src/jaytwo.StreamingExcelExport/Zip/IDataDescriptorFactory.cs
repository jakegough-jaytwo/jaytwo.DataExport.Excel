namespace jaytwo.StreamingExcelExport.Zip;

internal interface IDataDescriptorFactory
{
    IZipPart CreateDataDescriptor(
        uint crc32,
        long compressedSize,
        long uncompressedSize);
}
