using System.Text;

namespace jaytwo.StreamingExcelExport.Zip.Zip32;

internal class Zip32EndOfCentralDirectoryFactory
{
    public static Zip32EndOfCentralDirectory CreateEndOfCentralDirectory(
        int totalEntries,
        long centralDirectorySize,
        long centralDirectoryOffset,
        string? comment)
        => CreateEndOfCentralDirectoryFactory(
            (ushort)totalEntries,
            (uint)centralDirectorySize,
            (uint)centralDirectoryOffset,
            comment);

    public static Zip32EndOfCentralDirectory CreateEndOfCentralDirectoryFactory(
        ushort totalEntries = 0,
        uint centralDirectorySize = 0,
        uint centralDirectoryOffset = 0,
        string? comment = null)
    {
        comment ??= string.Empty;

        return new Zip32EndOfCentralDirectory
        {
            Signature = Zip32EndOfCentralDirectory.KnownSignature,
            DiskNumber = 0,
            CentralDirectoryStartDisk = 0,
            TotalEntriesOnThisDisk = totalEntries,
            TotalEntries = totalEntries,
            CentralDirectorySize = centralDirectorySize,
            CentralDirectoryOffset = centralDirectoryOffset,
            Comment = comment,
            CommentLength = (ushort)Encoding.UTF8.GetByteCount(comment),
        };
    }
}
