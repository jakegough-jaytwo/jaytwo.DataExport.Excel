namespace jaytwo.DataExport.Excel.Zip.Zip64;

internal class Zip64EndOfCentralDirectoryFactory
{
    public const ulong FixedSizeOfEOCD = 44;

    public static Zip64EndOfCentralDirectory CreateEndOfCentralDirectory(
        int totalEntries,
        long centralDirectorySize,
        long centralDirectoryOffset)
        => CreateEndOfCentralDirectoryFactory(
            (ulong)totalEntries,
            (ulong)centralDirectorySize,
            (ulong)centralDirectoryOffset);

    public static Zip64EndOfCentralDirectory CreateEndOfCentralDirectoryFactory(
        ulong totalEntries = 0,
        ulong centralDirectorySize = 0,
        ulong centralDirectoryOffset = 0)
    {
        return new Zip64EndOfCentralDirectory
        {
            Signature = Zip64EndOfCentralDirectory.KnownSignature,
            SizeOfEOCD = FixedSizeOfEOCD,
            VersionMadeBy = ZipConstants.Versions.Version45,
            VersionNeededToExtract = ZipConstants.Versions.Version45,
            DiskNumber = 0,
            CentralDirectoryStartDisk = 0,
            TotalEntriesOnThisDisk = totalEntries,
            TotalEntries = totalEntries,
            CentralDirectorySize = centralDirectorySize,
            CentralDirectoryOffset = centralDirectoryOffset,
        };
    }
}
