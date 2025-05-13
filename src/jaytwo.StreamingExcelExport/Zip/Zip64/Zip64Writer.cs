using System.IO;
using jaytwo.StreamingExcelExport.Zip.Zip32;

namespace jaytwo.StreamingExcelExport.Zip.Zip64;

internal class Zip64Writer : ZipWriter
{
    public Zip64Writer(Stream outputStream, bool leaveOpen = false, string? comment = null)
        : base(outputStream, leaveOpen, comment, new Zip64CentralDirectoryEntryFactory(), new Zip64DataDescriptorFactory(), new Zip64LocalFileHeaderFactory())
    {
    }

    protected override void WriteZipEndOfCentralDirectory(int totalEntries, long centralDirectorySize, long centralDirectoryOffset, string? comment)
    {
        /*
         * zip64: write entries, write Zip64EOCD, write Zip64EOCDLocator, write Legacy Zip32EOCD
         *
         * Most ZIP readers scan backward from the end of the file looking for:
         *   EOCD → if 0xFFFF/0xFFFFFFFF is found → fall back to:
         *   Locator → go to offset of:
         *   ZIP64 EOCD → read full ZIP64-compatible metadata
         */

        WriteToOutput(
            Zip64EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
                totalEntries: totalEntries,
                centralDirectoryOffset: centralDirectoryOffset,
                centralDirectorySize: centralDirectorySize),
            startPosition: out var zip64EndOfCentralDirectoryOffset);

        WriteToOutput(
            Zip64EndOfCentralDirectoryLocatorFactory.CreateLocator(zip64EndOfCentralDirectoryOffset));

        WriteToOutput(
            Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
                totalEntries: 0xFFFF,
                centralDirectoryOffset: 0xFFFFFFFF,
                centralDirectorySize: 0xFFFFFFFF,
                comment: comment));
    }
}
