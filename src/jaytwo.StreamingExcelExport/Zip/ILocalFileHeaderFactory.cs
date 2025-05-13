namespace jaytwo.StreamingExcelExport.Zip;

internal interface ILocalFileHeaderFactory
{
    IZipPart CreateLocalFileHeader(ushort compressionMethod, string fileName);
}
