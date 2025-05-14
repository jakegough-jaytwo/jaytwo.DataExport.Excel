namespace jaytwo.DataExport.Excel.Zip;

internal interface ILocalFileHeaderFactory
{
    IZipPart CreateLocalFileHeader(ushort compressionMethod, string fileName);
}
