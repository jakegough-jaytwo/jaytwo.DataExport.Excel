namespace jaytwo.DataExport.Excel.Zip.Zip64;

internal class Zip64EndOfCentralDirectoryLocatorFactory
{
    public static Zip64EndOfCentralDirectoryLocator CreateLocator(long zip64EndOfCentralDirectoryOffset)
        => CreateLocator((ulong)zip64EndOfCentralDirectoryOffset);

    public static Zip64EndOfCentralDirectoryLocator CreateLocator(ulong zip64EndOfCentralDirectoryOffset)
        => new Zip64EndOfCentralDirectoryLocator
        {
            Signature = Zip64EndOfCentralDirectoryLocator.KnownSignature,
            CentralDirectoryStartDisk = 0,
            TotalDisks = 1,
            Zip64EndOfCentralDirectoryOffset = zip64EndOfCentralDirectoryOffset,
        };
}
