using System;
using System.IO;

namespace jaytwo.DataExport.Excel.Zip.Zip32;

internal class Zip32Writer : ZipWriter
{
    public Zip32Writer(Stream outputStream, bool leaveOpen = false, string? comment = null)
        : base(outputStream, leaveOpen, comment, new Zip32CentralDirectoryEntryFactory(), new Zip32DataDescriptorFactory(), new Zip32LocalFileHeaderFactory())
    {
    }

    protected override void WriteZipEndOfCentralDirectory(int totalEntries, long centralDirectorySize, long centralDirectoryOffset, string? comment)
    {
        WriteToOutput(
            Zip32EndOfCentralDirectoryFactory.CreateEndOfCentralDirectory(
                totalEntries: totalEntries,
                centralDirectoryOffset: centralDirectoryOffset,
                centralDirectorySize: centralDirectorySize,
                comment: comment));
    }
}
